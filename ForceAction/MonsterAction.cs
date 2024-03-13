using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.IO;


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


        public unsafe void OnImGuiRender()
        {
            var monster = Monster.GetAllMonsters().LastOrDefault();

            var actionController = monster?.ActionController;

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
                if (monster == null)
                {
                    Log.Info($"No Monster.");
                    return;
                }    

                monster.ForceAction(actionId);
                Log.Info($"{monster?.Type} FORCED {actionId} {actionName}");
            }
        }
    }
}