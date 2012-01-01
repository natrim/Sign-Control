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

_protectallsigns_ - users can protect / unprotect all signs in region they have access to, ignoring all locks - protected signs get overwriten

 - _/sset_ _password_ REGION _region_name_
 - _/sunset_ _password_ REGION _region_name_

### Todo/Ideas:
- another access methods (region, owner, ...)
- lock reading
