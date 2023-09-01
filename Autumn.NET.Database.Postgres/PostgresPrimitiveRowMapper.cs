using Autumn.Database.Relational;

namespace Autumn.Database.Postgres;

public static class PostgresPrimitiveRowMapper {

    public static readonly RowMapper<byte> UInt8 = (result, _) => result.GetColumnValue(typeof(byte), 1) is byte i ? i : throw new Exception();

    public static readonly RowMapper<ushort> UInt16 = (result, _) => result.GetColumnValue(typeof(ushort), 1) is ushort i ? i : throw new Exception();

    public static readonly RowMapper<uint> UInt32 = (result, _) => result.GetColumnValue(typeof(uint), 1) is uint i ? i : throw new Exception();

    public static readonly RowMapper<ulong> UInt64 = (result, _) => result.GetColumnValue(typeof(ulong), 1) is ulong i ? i : throw new Exception();

    public static readonly RowMapper<sbyte> Int8 = (result, _) => result.GetColumnValue(typeof(sbyte), 1) is sbyte i ? i : throw new Exception();

    public static readonly RowMapper<short> Int16 = (result, _) => result.GetColumnValue(typeof(short), 1) is short i ? i : throw new Exception();

    public static readonly RowMapper<int> Int32 = (result, _) => result.GetColumnValue(typeof(int), 1) is int i ? i : throw new Exception();

    public static readonly RowMapper<long> Int64 = (result, _) => result.GetColumnValue(typeof(long), 1) is long i ? i : throw new Exception();

    public static readonly RowMapper<bool> Boolean = (result, _) => result.GetColumnValue(typeof(bool), 1) is bool i ? i : throw new Exception();

}
