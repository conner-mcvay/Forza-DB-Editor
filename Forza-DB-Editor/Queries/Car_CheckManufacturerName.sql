SELECT COUNT(*) AS column_exists
FROM pragma_table_info('List_CarMake')
WHERE name = 'IconPathBase';