
REM MS SQL Express 2005 has no GUI !
REM You can use this script to install the tables

set SQLCMDUSER=sa
set SQLCMDPASSWORD=

sqlcmd -S FM071\SQLEXPRESS -d Test -i mssqlserver.sql -o mssqlserver.log
