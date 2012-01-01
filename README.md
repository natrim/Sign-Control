## Sign-Control
Control your signs.

### Usage:
_/sset_ _password_ - Protects sign from editing - to edit you must unlock it using _/sunlock_ command

_/sunset_ _password_ - Removes protection of sign

_/scset_ - Stops selecting of sign

_/sunlock_  _password_ -  Unlocks signs for editing until logout

### Permissions:
_canunlocksign_ - permission for _/sunlock_ command - on install added to default register group

_protectsign_ - can protect signs

_editallsigns_ - permission for admins - can edit even protected signs

_removesignprotection_ - permission for admins - can remove all signs protections

_protectallsigns_ - meant for admins - can protect / unprotect all signs in region they have build access to

 - _/sset_ REGION _region_name_ _password_ OR _/sset_ REGION _region_name_ _password_ TRUE		* TRUE or empty overwrites all signs in region
 - _/sset_ REGION _region_name_ _password_ FALSE		* using FALSE protects only unprotected signs
 - _/sunset_ REGION _region_name_		* without password removes from all signs
 - _/sunset_ REGION _region_name_ _password_		* with password removes protection only from signs with this password

### Todo/Ideas:
- another access methods (region, owner, ...)
- lock reading
