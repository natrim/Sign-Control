using System.Collections.Generic;

namespace SignControl
{
	public class SPlayer
	{
		public string PasswordForSign = "";
		public string WarpForSign = "";
		protected SettingState State = SettingState.None;
		protected List<int> UnlockedSigns = new List<int> ();

		public SPlayer ()
		{
		}

		public SettingState GetState ()
		{
			return State;
		}

		public void SetState (SettingState state)
		{
			State = state;
		}

		public void AddSignAccess (int id)
		{
			UnlockedSigns.Add (id);
			PasswordForSign = "";
		}

		public void RemoveSignAccess (int id)
		{
			if (UnlockedSigns.Contains (id))
				UnlockedSigns.Remove (id);

			PasswordForSign = "";
		}

		public bool CanEditSign (int id)
		{
			return UnlockedSigns.Contains (id);
		}
		
		public static void RemoveSignAccessFromAll (int id)
		{
			foreach (SPlayer pl in SignControl.Players)
				pl.RemoveSignAccess (id);
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