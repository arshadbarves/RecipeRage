namespace Playcenter.Services
{
    public readonly struct CurrencyId : System.IEquatable<CurrencyId>
    {
        public string Value { get; }
        public CurrencyId(string value) => Value = value ?? string.Empty;
        public static CurrencyId Coins { get; } = new("coins");
        public static CurrencyId Gems { get; } = new("gems");
        public bool Equals(CurrencyId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is CurrencyId c && Equals(c);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override string ToString() => Value;
        public static bool operator ==(CurrencyId a, CurrencyId b) => a.Equals(b);
        public static bool operator !=(CurrencyId a, CurrencyId b) => !a.Equals(b);
    }
}
