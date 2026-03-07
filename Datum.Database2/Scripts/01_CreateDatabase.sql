-- ============================================================
-- Datum.Database
-- Script 01: Criação do banco de dados
-- Execute este script conectado ao SQL Server (master ou outra db)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'datum')
BEGIN
    CREATE DATABASE [datum]
        CONTAINMENT = NONE
        ON PRIMARY
        (
            NAME = N'datum',
            FILENAME = N'datum.mdf',
            SIZE = 64MB,
            MAXSIZE = UNLIMITED,
            FILEGROWTH = 64MB
        )
        LOG ON
        (
            NAME = N'datum_log',
            FILENAME = N'datum_log.ldf',
            SIZE = 16MB,
            MAXSIZE = 2048GB,
            FILEGROWTH = 16MB
        );

    PRINT 'Database [datum] criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Database [datum] já existe. Nenhuma ação necessária.';
END
GO

ALTER DATABASE [datum] SET RECOVERY SIMPLE;
GO

USE [datum];
GO
