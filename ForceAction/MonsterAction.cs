using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using System.Threading;
using System.Runtime.CompilerServices;


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
        public void OnLoad() 
        {
            KeyBindings.AddKeybind("DoIt", new Keybind<Key>(Key.Z, [Key.LeftShift]));
        }

        private void ResetState()
        {
            _selectedMonsterA = null;
            _lastStage = (uint)Area.CurrentStage;
        }
        public void OnUpdate(float dt)
        {
            if ((uint)Area.CurrentStage != _lastStage)
            {
                ResetState();
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
            if (ImGui.BeginCombo("LShift Z", $"{_selectedMonsterA}"))
            {
                foreach (var monster in monsters)
                {
                    //if (ImGui.Selectable($"{monster.Name}", _selectedMonsterA == monster))
                    if (ImGui.Selectable($"{monster}", _selectedMonsterA == monster))
                    {
                        _selectedMonsterA = monster;
                    }
                }
                ImGui.EndCombo();
            }

            if (_selectedMonsterA == null)
                return;

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

            if (KeyBindings.IsPressed("DoIt") || ImGui.Button("Action##monster"))
            {
                if (_selectedMonsterA == null)
                    return;

                _selectedMonsterA.ForceAction(actionId);
                Log.Info($"{_selectedMonsterA} FORCED {actionId} {actionName}");
            }

            if (ImGui.Button("Enrage"))
            {
                if (_selectedMonsterA == null) return;
                _selectedMonsterA.Enrage();
            }
            if (ImGui.Button("Unenrage"))
            {
                if (_selectedMonsterA == null) return;
                _selectedMonsterA.Unenrage();
            }


            /*
            nint setTarget = _setTarget;
            if (ImGui.InputScalar("Target nint", ImGuiDataType.S32, new IntPtr(Unsafe.AsPointer(ref setTarget))))
            {
                _setTarget = setTarget;
            }
            var me = Player.MainPlayer;
            if (me == null) return;
            nint myNint = me.Instance;
            if (ImGui.Button("Target Me"))
            {
                _selectedMonsterA.SetTarget(myNint);
            }*/
        }
    }
}