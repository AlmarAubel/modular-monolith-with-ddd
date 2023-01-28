#!/bin/bash
echo "$MSSQL_SA_PASSWORD"
echo 'Waiting for SQL server started.';

sleep 20 ;

echo 'Create database..';

# Create database
/opt/mssql-tools/bin/sqlcmd -S mymeetingsdb -d master -i /scripts/CreateDatabase_Linux.sql -U sa -P Test@12345 ;

echo 'Database created';