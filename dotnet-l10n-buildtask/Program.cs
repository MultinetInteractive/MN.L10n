using System.Threading.Tasks;

namespace dotnet_l10n_buildtask
{
    static class Program
    {
        async static Task<int> Main(string[] args)
        {
            return await MN.L10n.BuildTasks.Program.Main(args);
        }
    }
}
