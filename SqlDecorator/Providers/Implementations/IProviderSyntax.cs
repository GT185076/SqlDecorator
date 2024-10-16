using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLDecorator.Providers.Implementations
{
    public interface IProviderSyntax
    {
        internal string Top(int Max);
        internal string Limit(int Max);
    }

    public class MssqlSyntax : IProviderSyntax
    {
        string IProviderSyntax.Limit(int Max)
        {
            return string.Empty;
        }

        string IProviderSyntax.Top(int Max)
        {
            return "Top("+Max.ToString()+")";
        }
    }
    public class PostGresSyntax : IProviderSyntax
    {
        string IProviderSyntax.Limit(int Max)
        {
            return string.Empty;
        }

        string IProviderSyntax.Top(int Max)
        {
            return "Top(" + Max.ToString() + ")";
        }
    }

    public class SqlLiteSyntax : IProviderSyntax
    {
        string IProviderSyntax.Limit(int Max)
        {
            return "Limit " + Max.ToString();
        }

        string IProviderSyntax.Top(int Max)
        {
            return string.Empty;
        }
    }

}
