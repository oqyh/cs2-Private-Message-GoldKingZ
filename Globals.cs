using CounterStrikeSharp.API.Core;
using System.Diagnostics;

namespace PrivateMessageGoldKingZ;

public class Globals
{
    public static int CurrentRoomAvailable = 0;
    public static Dictionary<ulong, bool> spy_dm = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> spy_pr = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> dm_group = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> pr_group = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, int> CreatingPrivateRoom = new Dictionary<ulong, int>();
    public static Dictionary<ulong, int> Watingforaccept = new Dictionary<ulong, int>();
}