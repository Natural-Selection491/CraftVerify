using DataAccessLibraryCraftVerify;

static void Main()
{
    string SqlServerconnString = "Server=localhost;Database=CraftVerify;User Id=admin;Password=admin;TrustServerCertificate=true";
    //string MySqlServerconnString = "@Server=myServerAddress;Database=myDataBase;User=myUsername;Password=myPassword;\r\n";

    string INSERTsqlcommand = "INSERT INTO ClaimPrinciple (claim, stuff, hashPrinciple) VALUES ('admin', 'admin', 'aikjwnrvikhjqb3nrb');";
    string GETsqlcommand = "SELECT * FROM ClaimPrinciple WHERE claim = 'admin';";
    SQLServerDAO sqlDAO = new SQLServerDAO();

    sqlDAO.InsertAttribute(SqlServerconnString, INSERTsqlcommand);

    Console.WriteLine(sqlDAO.GetAttribute(SqlServerconnString, GETsqlcommand));
}

Main();

/*
 * 
 * CREATE TABLE [dbo].[UserAccount] (
    [userID]                           CHAR (10)    NOT NULL,
    [email]                            VARCHAR (64) NULL,
    [userHash]                         CHAR (64)    NOT NULL,
    [hashedOTP]                        CHAR (64)    NULL,
    [otpSalt]                          CHAR (10)    NULL,
    [userStatus]                       BIT NULL,
    [dateCreate]                       DATETIME     NULL,
    [secureAnswer1]                    VARCHAR (50) NULL,
    [secureAnswer2]                    VARCHAR (50) NULL,
    [secureAnswer3]                    VARCHAR (50) NULL,
    [firstAuthenticationFailTimestamp] DATETIME     NULL,
    [failedAuthenticationAttempts]     INT          NULL,
    PRIMARY KEY CLUSTERED ([userHash] ASC)
);

CREATE TABLE[dbo].[LogTable]([logID] BIGINT        NOT NULL,[userHash] CHAR(64)     NULL,[actionType] VARCHAR(50)  NULL,[logTime]DATETIME NULL,[logStatus]  VARCHAR(50)  NULL,[logDetail] VARCHAR(200) NULL,PRIMARY KEY CLUSTERED([logID] ASC));

CREATE TABLE[dbo].[ClaimPrinciple] ([claim] VARCHAR (64) NOT NULL,[identity]      VARCHAR(64) NULL,[hashPrinciple] CHAR(64)    NULL,PRIMARY KEY CLUSTERED([claim] ASC));

CREATE TABLE[dbo].[UserProfile]([userID] CHAR (10)      NOT NULL,[userHash]        CHAR(64)      NOT NULL,[profileUserRole] VARCHAR(10)   NULL,[profileUsername] VARCHAR(64)   NULL,[profileDOB]DATETIME NULL,[profileBio]      NVARCHAR(500) NULL,[profilePhoto]IMAGE NULL,PRIMARY KEY CLUSTERED([userHash] ASC));

*/