using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using System.Threading;


namespace MonsterAction
{
    public class MonsterAction : IPlugin
    {
        public string Name => "Monster Action";
        public string Author => "Seka";


        public void OnLoad() 
        {
            KeyBindings.AddKeybind("DoIt", new Keybind<Key>(Key.Z, [Key.LeftShift]));
        }

        private int _selectedActionM;

        private Monster? _selectedMonsterA = null;

        public void OnMonsterDestroy(Monster monster) {
            if (monster == _selectedMonsterA) { _selectedMonsterA = null; }
        }
        public void OnMonsterDeath(Monster monster) {
            if (monster == _selectedMonsterA) { _selectedMonsterA = null; }
        }

        public unsafe void OnImGuiRender()
        {
            var monsters = Monster.GetAllMonsters().TakeLast(5).ToArray();
            if (monsters == null)
                return;
            if (ImGui.BeginCombo("Monster Act", $"{_selectedMonsterA}"))
            {
                foreach (var monster in monsters)
                {
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
            var actionName = (action is null || action.Instance == 0) ? "N/A" : action.Name;
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

            if (KeyBindings.IsPressed("DoIt") || ImGui.Button("Press to Act##monster"))
            {
                if (_selectedMonsterA == null)
                    return;

                _selectedMonsterA.ForceAction(actionId);
                Log.Info($"{_selectedMonsterA} FORCED {actionId} {actionName}");
            }
        }
    }
}