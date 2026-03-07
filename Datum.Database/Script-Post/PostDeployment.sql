/*
Modelo de Script Pós-Implantação							
--------------------------------------------------------------------------------------
 Este arquivo contém instruções SQL que serão acrescentadas ao script de compilação.		
 Use sintaxe SQLCMD para incluir um arquivo no script pós-implantação.			
 Exemplo:      :r .\myfile.sql								
 Use sintaxe SQLCMD para referenciar uma variável no script pós-implantação.		
 Exemplo:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

:r .\Post.Initialize.sql
:r .\User.Initialize.sql
