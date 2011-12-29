namespace SignControl
{
	public static class Messages
	{
		public static readonly string isProtected = "This sign is protected!";
		
		public static readonly string alreadyProtected = "This sign is already protected!";
		
		public static readonly string nowProtected = "This sign is now protected.";
		
		public static readonly string notProtected = "This sign is not protected!";
		
		public static readonly string removeSign = "To remove sign, first remove protection using \"/sunset\" command.";
		
		public static readonly string removed = "Sign protection removed!";
		
		public static readonly string wrongPassword = "Wrong password!";
		
		public static readonly string enterPassword = "You must enter password!";
		
		public static readonly string password = "( If you know the password, you can unlock this sign using \"/sunlock PASSWORD\" command. )";
		
		public static readonly string unlocked = "Sign editing unlocked!";
		
		public static readonly string noEdit = "You don't have permission to edit this sign!";
		public static readonly string stopEdit = "This sign is protected with password. Your changes would be not visible to other players.";
		public static readonly string editable = "This sign is protected. You are able to edit it.";
		public static readonly string notEditable = "This sign is protected. You are not able to edit it.";
		
		
		public static readonly string warpTo = "This sign will now warp to: {0}";
		public static readonly string noWarp = "This sign is not warp-enabled.";
		public static readonly string notWarping = "This sign is no longer warping.";
		public static readonly string invalidWarp = "That is an invalid warp name!";
		public static readonly string enterWarp = "You must enter a warp the sign warps to!";
		
		public static readonly string teleported = "You have been teleported to: {0}";
		public static readonly string wrongWarp = "You have been not teleported, because warp no longer exists!";
		
		
		public static readonly string stopSelecting = "You are no longer selecting a sign.";
		
		public static readonly string notNeedUnlock = "You dont need to unlock signs because you have permission to edit all signs!";
		
		public static readonly string openSignTo = "Open a sign to {0}.";
		public static readonly string protect = "protect it";
		public static readonly string unlock = "unlock it";
		public static readonly string delete = "delete it's protection";
		public static readonly string warpable = "make it warp-able";
		public static readonly string unwarpable = "delete its warp";
		
		
	}
}
