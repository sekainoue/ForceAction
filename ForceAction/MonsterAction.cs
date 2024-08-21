using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using System.Threading;
using System.Runtime.CompilerServices;
using SharpPluginLoader.Core.MtTypes;


namespace MonsterAction
{
    public class MonsterAction : IPlugin
    {
        public string Name => "Monster Action";
        public string Author => "Seka";


        private int _selectedActionM;
        private Monster? _selectedMonsterA = null;
        private uint _lastStage = 0;
        private bool _enraged = false;
        private nint _setTarget;
        private float _newRotationX;
        private float _newRotationY;
        private float _newRotationZ;
        public bool _targetOn = false;
        public void OnLoad()
        {
            KeyBindings.AddKeybind("DoIt", new Keybind<Key>(Key.Z, [Key.LeftShift]));
        }

        private void ResetState()
        {
            _selectedMonsterA = null;
        }
        public void OnUpdate(float dt)
        {
            if ((uint)Area.CurrentStage != _lastStage)
            {
                ResetState();
            }

            var me = Player.MainPlayer;
            if (me == null) return;
            nint myNint = me.Instance;
            if (_targetOn && _selectedMonsterA != null)
            {
                _selectedMonsterA.SetTarget(myNint);
            }
        }
        public void OnQuestLeave(int questId) { ResetState(); }
        public void OnQuestComplete(int questId) { ResetState(); }
        public void OnQuestFail(int questId) { ResetState(); }
        public void OnQuestReturn(int questId) { ResetState(); }
        public void OnQuestAbandon(int questId) { ResetState(); }
        public void OnQuestEnter(int questId) { ResetState(); }
        public void OnMonsterDestroy(Monster monster) { if (monster == _selectedMonsterA) { _selectedMonsterA = null; } }
        public void OnMonsterDeath(Monster monster) { if (monster == _selectedMonsterA) { _selectedMonsterA = null; } }

        public unsafe void OnImGuiRender()
        {
            var monsters = Monster.GetAllMonsters().TakeLast(8).ToArray();
            if (monsters == null)
                return;

            if (ImGui.Button("Unset"))
            {
                _selectedMonsterA = null;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(200.0f);
            if (ImGui.BeginCombo("Monster", $"{_selectedMonsterA}"))
            {
                foreach (var monster in monsters)
                {
                    //if (ImGui.Selectable($"{monster.Name}", _selectedMonsterA == monster))
                    if (ImGui.Selectable($"{monster}", _selectedMonsterA == monster))
                    {
                        _selectedMonsterA = monster;
                        _lastStage = (uint)Area.CurrentStage;
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            if (_selectedMonsterA == null)
                return;

            ImGui.SameLine();
            if (ImGui.Button("Enrage"))
            {
                if (_selectedMonsterA == null) return;
                _selectedMonsterA.Enrage();
            }

            ImGui.SameLine();
            if (ImGui.Button("Unenrage"))
            {
                if (_selectedMonsterA == null) return;
                _selectedMonsterA.Unenrage();
            }

            var actionController = _selectedMonsterA.ActionController;
            if (actionController == null)
                return;

            var secondActionListM = actionController.GetActionList(1); // selects List B

            if (_selectedActionM >= secondActionListM.Count)
                _selectedActionM = 0;

            if (secondActionListM.Count == 0)
                return;


            var action = secondActionListM[_selectedActionM];
            var actionName = action?.Name ?? "N/A";
            int actionId = _selectedActionM;

            ImGui.PushItemWidth(250.0f);
            if (ImGui.BeginCombo("Lshift + Z", $"{actionId} {actionName}"))
            {
                for (var l = 0; l < secondActionListM.Count; ++l)
                {
                    action = secondActionListM[l];
                    if (action is null || action.Instance == 0)
                        continue;

                    if (ImGui.Selectable($"{l} {action.Name}", l == _selectedActionM))
                    {
                        _selectedActionM = l;
                    }

                    if (l == _selectedActionM)
                    {
                        ImGui.SetItemDefaultFocus();
                    }

                    actionId = _selectedActionM;
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            if (KeyBindings.IsPressed("DoIt") || ImGui.Button("Action##monster"))
            {
                if (_selectedMonsterA == null)
                    return;

                _selectedMonsterA.ForceAction(actionId);
                Log.Info($"{_selectedMonsterA} FORCED {actionId} {actionName}");
            }

            ImGui.SameLine();
            if (ImGui.Button("AllDo"))
            {
                foreach (var monster in monsters)
                {
                    monster.ForceAction(actionId);
                }
            }

            if (ImGui.Button("0"))
            {
                _selectedMonsterA.Rotation.X = 0f;
                _selectedMonsterA.Rotation.Y = 0f;
                _selectedMonsterA.Rotation.Z = 0f;
            }
            ImGui.PushItemWidth(150.0f);
            if (ImGui.SliderFloat("X", ref _newRotationX, -180f, 180f))
            {
                _selectedMonsterA.Rotation.X = _newRotationX / 100f;
            }
            ImGui.SameLine();
            if (ImGui.SliderFloat("Y", ref _newRotationY, -180f, 180f))
            {
                _selectedMonsterA.Rotation.Y = _newRotationY / 100f;
            }
            ImGui.SameLine();
            if (ImGui.SliderFloat("Z", ref _newRotationZ, -180f, 180f))
            {
                _selectedMonsterA.Rotation.Z = _newRotationZ / 100;
            }
            ImGui.PopItemWidth();


            

            nint setTarget = _setTarget;
            if (ImGui.InputScalar("Target nint", ImGuiDataType.S64, MemoryUtil.AddressOf(ref setTarget))) // nint is a 64 bit integer
                // MemoryUtil.AddressOf(ref setTarget)
                // is the same as
                // new IntPtr(Unsafe.AsPointer(ref setTarget)
            {
                _setTarget = setTarget;
            }
            
            if (ImGui.Button("Target Me"))
            {
                _targetOn = true;
            }
        }
    }
}