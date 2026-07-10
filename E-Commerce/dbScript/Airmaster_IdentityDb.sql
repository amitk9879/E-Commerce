USE [master]
GO
/****** Object:  Database [Airmaster_IdentityDb]    Script Date: 10-07-2026 15:15:25 ******/
CREATE DATABASE [Airmaster_IdentityDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Airmaster_IdentityDb', FILENAME = N'C:\Users\amitk\Airmaster_IdentityDb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Airmaster_IdentityDb_log', FILENAME = N'C:\Users\amitk\Airmaster_IdentityDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Airmaster_IdentityDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Airmaster_IdentityDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET  MULTI_USER 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Airmaster_IdentityDb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Airmaster_IdentityDb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
USE [Airmaster_IdentityDb]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 10-07-2026 15:15:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 10-07-2026 15:15:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](150) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Role] [nvarchar](max) NOT NULL,
	[RefreshToken] [nvarchar](max) NULL,
	[RefreshTokenExpiryTime] [datetime2](7) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260707200448_InitialCreate', N'10.0.9')
GO
INSERT [dbo].[Users] ([Id], [Email], [PasswordHash], [FirstName], [LastName], [Role], [RefreshToken], [RefreshTokenExpiryTime]) VALUES (N'a1111111-2222-3333-4444-555555555555', N'hvac.engineer@daikin.com', N'$2a$12$XF9cDHq1uaA0IyXs7DFoGO0/Qw6a.S.TFswTJhVsnUPI6mgNH4tWa', N'Rahul', N'Sharma', N'Customer', N'xojNTnhy1aHyPGIHbXdTl7ZmikSvUAB6mG3cp9kQnwWX75KvbeE9R9eDXoKT9lx5InVOv+MotupkEgZGSWN1nA==', CAST(N'2026-07-17T09:08:23.8940143' AS DateTime2))
GO
INSERT [dbo].[Users] ([Id], [Email], [PasswordHash], [FirstName], [LastName], [Role], [RefreshToken], [RefreshTokenExpiryTime]) VALUES (N'b1111111-2222-3333-4444-555555555555', N'admin.portal@daikin.com', N'$2a$12$XF9cDHq1uaA0IyXs7DFoGO0/Qw6a.S.TFswTJhVsnUPI6mgNH4tWa', N'Sanjay', N'Mehta', N'Admin', N'ttmENLEm1kVoLzgr6gQ86hQMd1bJ87nm6t00Ab2mTUXb16nOm/8Weq9n3mJ8bbC7gGvFVK2A4Ttg9dltR5HZLw==', CAST(N'2026-07-17T08:49:28.9785956' AS DateTime2))
GO
INSERT [dbo].[Users] ([Id], [Email], [PasswordHash], [FirstName], [LastName], [Role], [RefreshToken], [RefreshTokenExpiryTime]) VALUES (N'c3333333-2222-3333-4444-555555555555', N'contractor@delhi-hvac.in', N'$2a$12$XF9cDHq1uaA0IyXs7DFoGO0/Qw6a.S.TFswTJhVsnUPI6mgNH4tWa', N'Amit', N'Verma', N'Customer', N'lw1psGynzXJbOiu6rk+2cAQnzyBbmBpoELclX5ns1REva1R/CWXfIKrHICm6Tr/HP1Zreuy6kreXgs2q6NolAA==', CAST(N'2026-07-17T06:27:33.3403125' AS DateTime2))
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 10-07-2026 15:15:25 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [Airmaster_IdentityDb] SET  READ_WRITE 
GO
