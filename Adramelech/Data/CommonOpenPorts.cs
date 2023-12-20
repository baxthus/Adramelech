namespace Adramelech.Data;

public static class CommonOpenPorts
{
    public static readonly List<int> Ports =
    [
        // List from https://www.speedguide.net/ports_common.php (accessed on 19/12/2023)
        // The list maybe be shorted in the future for performance and resource usage reasons
        // While the bot is private, this is not a problem
        443, 22, 5060, 53, 8080, 1723, 21, 3389, 8000, 8081, 4567, 25, 8082, 23, 81, 10000, 993, 5000, 445, 995, 110,
        143, 111, 7547, 135, 139, 28960, 29900, 18067, 27374, 4444, 1024, 7676, 389, 1025, 1026, 30005, 20, 5678, 1027,
        1050, 1028, 1029, 8594, 1863, 3783, 1002, 4664, 37
    ];
}