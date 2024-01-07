using Autumn.Database.Relational;

namespace Autumn.Database.Postgres;

public static class PostgresPrimitiveRowMapper {

    public static readonly RowMapper<byte> UInt8 = (result, _) => result.GetColumnValue(typeof(byte), 0) is byte i ? i : throw new Exception();

    public static readonly RowMapper<ushort> UInt16 = (result, _) => result.GetColumnValue(typeof(ushort), 0) is ushort i ? i : throw new Exception();

    public static readonly RowMapper<uint> UInt32 = (result, _) => result.GetColumnValue(typeof(uint), 0) is uint i ? i : throw new Exception();

    public static readonly RowMapper<ulong> UInt64 = (result, _) => result.GetColumnValue(typeof(ulong), 0) is ulong i ? i : throw new Exception();

    public static readonly RowMapper<sbyte> Int8 = (result, _) => result.GetColumnValue(typeof(sbyte), 0) is sbyte i ? i : throw new Exception();

    public static readonly RowMapper<short> Int16 = (result, _) => result.GetColumnValue(typeof(short), 0) is short i ? i : throw new Exception();

    public static readonly RowMapper<int> Int32 = (result, _) => result.GetColumnValue(typeof(int), 0) is int i ? i : throw new Exception();

    public static readonly RowMapper<long> Int64 = (result, _) => result.GetColumnValue(typeof(long), 0) is long i ? i : throw new Exception();

    public static readonly RowMapper<bool> Boolean = (result, _) => result.GetColumnValue(typeof(bool), 0) is bool i ? i : throw new Exception();

}
