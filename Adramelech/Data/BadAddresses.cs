namespace Adramelech.Data;

// https://en.wikipedia.org/wiki/Special-use_domain_name
// https://www.arin.net/reference/research/statistics/address_filters
public static class BadAddresses
{
    public static readonly List<string> Hosts =
    [
        // Basic hostnames
        "localhost", "router", "gateway",
        // Private/internal DNS Namespaces
        "intranet", "internal", "private", "corp", "home", "lan", "home",
        // Used for Multicast DNS
        "local",
        // Onion services
        "onion",
        // Network testing
        "test",
        // Reserved Ipv4
        "10.", "172.16.", "172.17.", "172.18.", "172.19.", "172.20.", "172.21.", "172.22.", "172.23.", "172.24.",
        "172.25.", "172.26.", "172.27.", "172.28.", "172.29.", "172.30.", "172.31.", "192.168.", "127.",
    ];

    public static readonly List<string> TlDs =
        ["local", "onion", "test", "arpa", "example", "invalid", "localhost", "test"];
}