using System.Collections.Generic;
using TShockAPI;

namespace SignControl
{
    public class SPlayer : TSPlayer
    {
        public string PasswordForSign = "";
        protected SettingState State = SettingState.None;
        protected List<int> UnlockedSigns = new List<int>();
        public string WarpForSign = "";

        public SPlayer(int index)
            : base(index)
        {
        }

        public SettingState GetState()
        {
            return State;
        }

        public void SetState(SettingState state)
        {
            State = state;
        }

        public void AddSignAccess(int id)
        {
            UnlockedSigns.Add(id);
            PasswordForSign = "";
        }

        public void RemoveSignAccess(int id)
        {
            if (UnlockedSigns.Contains(id))
                UnlockedSigns.Remove(id);

            PasswordForSign = "";
        }


        public bool CanEditSign(int id)
        {
            return UnlockedSigns.Contains(id);
        }
    }

    public enum SettingState
    {
        None,
        Setting,
        Deleting,
        UnLocking,
        WarpSetting,
        DeletingWarp
    }
}