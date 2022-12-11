CREATE PROCEDURE [dbo].[adm_AddLoginUser]
@LoginName sysname,
@RoleToGrant sysname
AS
	set @LoginName = dbo.adm_FormatNTGroupName(@LoginName)
	declare @NeedGrantLogin as int, @NeedGrantDbAccess as int, @NeedGrantDbRole as int, @IsMember as int
	select @NeedGrantLogin = 0, @NeedGrantDbAccess = 0, @NeedGrantDbRole = 0, @IsMember = 0
	
	-- Add SQL login if it doesn't already exist
	if not exists (select * from master.dbo.syslogins where sid = suser_sid(@LoginName))  
		set @NeedGrantLogin = 1
	
	-- Add database uesr if it doesn't already exist
	if not exists (select * from sysusers where sid = suser_sid(@LoginName) and hasdbaccess = 1)
		set @NeedGrantDbAccess = 1
	-- If the user don't have sufficient permission, then throw an error and return immediately
	if ( @NeedGrantLogin = 1 AND dbo.adm_HasPermissionToPerform('LoginTasks') = 0 )
		return 0xC0C025CF -- CIS_E_ADMIN_CORE_SQL_LOGIN_CREATION_INSUFFICIENT_PRIV
	if ( @NeedGrantDbAccess = 1 AND dbo.adm_HasPermissionToPerform('DbAccessTasks') = 0 )
		return 0xC0C025D2 -- CIS_E_ADMIN_CORE_SQL_DBACCESS_OPS_INSUFFICIENT_PRIV
	-- Create the SQL login
	if ( @NeedGrantLogin = 1 )
	begin
		if exists (select * from master.dbo.syslogins where name = @LoginName)
			return 0xC0C02600 -- CIS_E_ADMIN_CORE_SQL_DBROLE_OPS_STALE_ENTRY
			--exec sp_revokelogin @LoginName
		exec sp_grantlogin @LoginName
	end
	-- Grant database access
	if ( @NeedGrantDbAccess = 1 )
	begin
		if exists (select * from sysusers where name = @LoginName)
			return 0xC0C02600 -- CIS_E_ADMIN_CORE_SQL_DBROLE_OPS_STALE_ENTRY
			--exec sp_revokedbaccess @LoginName
		exec sp_grantdbaccess @LoginName
	end
	-- Grant role membership or not?
	if ( LEN(@RoleToGrant) > 0 )
	begin
		exec adm_IsMemberOfRole @LoginName, @RoleToGrant, @IsMember output
		if ( @IsMember = 0 )
			set @NeedGrantDbRole = 1
	end
	if ( @NeedGrantDbRole = 1 AND dbo.adm_HasPermissionToPerform('DbRoleTasks') = 0 )
		return 0xC0C025D3 -- CIS_E_ADMIN_CORE_SQL_DBROLE_OPS_INSUFFICIENT_PRIV
	
	-- Conditionally grant SQL role membership
	if ( @NeedGrantDbRole = 1 )
		exec adm_AddDbUserToRole @LoginName, @RoleToGrant
