SET NOCOUNT ON;

DECLARE @Email nvarchar(256) = N'REPLACE_WITH_REGISTERED_EMAIL';
DECLARE @RoleName nvarchar(256) = N'Editor'; -- Change to Admin when needed.
DECLARE @UserId nvarchar(450);
DECLARE @RoleId nvarchar(450);

IF @RoleName NOT IN (N'Editor', N'Admin')
    THROW 50001, 'Choose either the Editor or Admin role.', 1;

SELECT @UserId = Id
FROM dbo.AspNetUsers
WHERE Email = @Email;

IF @UserId IS NULL
    THROW 50002, 'Register this email address before assigning a role.', 1;

SELECT @RoleId = Id
FROM dbo.AspNetRoles
WHERE Name = @RoleName;

IF @RoleId IS NULL
    THROW 50003, 'The role does not exist. Start the API after applying migrations first.', 1;

BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.AspNetUserRoles
    WHERE UserId = @UserId AND RoleId = @RoleId
)
BEGIN
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END;

UPDATE dbo.AspNetUsers
SET SecurityStamp = CONVERT(nvarchar(36), NEWID())
WHERE Id = @UserId;

COMMIT TRANSACTION;
