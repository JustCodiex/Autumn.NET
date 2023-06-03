namespace Autumn.Database.Relational;

public delegate T RowMapper<T>(QueryResult result, int rowIndex);

public delegate object GeneratedRowMapper(QueryResult result, int rowIndex);
