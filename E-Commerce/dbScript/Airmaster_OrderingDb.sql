USE [master]
GO
/****** Object:  Database [Airmaster_OrderingDb]    Script Date: 10-07-2026 15:18:56 ******/
CREATE DATABASE [Airmaster_OrderingDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Airmaster_OrderingDb', FILENAME = N'C:\Users\amitk\Airmaster_OrderingDb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Airmaster_OrderingDb_log', FILENAME = N'C:\Users\amitk\Airmaster_OrderingDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Airmaster_OrderingDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Airmaster_OrderingDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET  MULTI_USER 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Airmaster_OrderingDb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Airmaster_OrderingDb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
USE [Airmaster_OrderingDb]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 10-07-2026 15:18:56 ******/
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
/****** Object:  Table [dbo].[OrderItems]    Script Date: 10-07-2026 15:18:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItems](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[ProductId] [uniqueidentifier] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[ProductName] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 10-07-2026 15:18:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[OrderDate] [datetime2](7) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](max) NOT NULL,
	[ShippingAddress] [nvarchar](max) NOT NULL,
	[TransactionId] [nvarchar](max) NULL,
	[TrackingNumber] [nvarchar](max) NULL,
	[Carrier] [nvarchar](max) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OutboxMessages]    Script Date: 10-07-2026 15:18:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OutboxMessages](
	[Id] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[OccurredOnUtc] [datetime2](7) NOT NULL,
	[ProcessedOnUtc] [datetime2](7) NULL,
	[Error] [nvarchar](max) NULL,
 CONSTRAINT [PK_OutboxMessages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItems_OrderId]    Script Date: 10-07-2026 15:18:56 ******/
CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId] ON [dbo].[OrderItems]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_UserId]    Script Date: 10-07-2026 15:18:56 ******/
CREATE NONCLUSTERED INDEX [IX_Orders_UserId] ON [dbo].[Orders]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_OutboxMessages_ProcessedOnUtc]    Script Date: 10-07-2026 15:18:56 ******/
CREATE NONCLUSTERED INDEX [IX_OutboxMessages_ProcessedOnUtc] ON [dbo].[OutboxMessages]
(
	[ProcessedOnUtc] ASC
)
WHERE ([ProcessedOnUtc] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[OrderItems] ADD  DEFAULT (N'') FOR [ProductName]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Orders_OrderId]
GO
USE [master]
GO
ALTER DATABASE [Airmaster_OrderingDb] SET  READ_WRITE 
GO
