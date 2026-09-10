// © CluedIn ApS. All rights reserved. CluedIn® is a registered trademark of CluedIn ApS.

using System.Diagnostics.CodeAnalysis;
using Nager.PublicSuffix;
#if CLUEDIN_V50
using Nager.PublicSuffix.Exceptions;
using Nager.PublicSuffix.RuleProviders;
#endif

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Net;

internal static class DomainName
{
    // Nager.PublicSuffix major-version break: CluedIn 4.7/4.8 (net6.0) resolve Nager.PublicSuffix
    // 2.4.0 (flat Nager.PublicSuffix namespace, WebTldRuleProvider); CluedIn 5.0+ (net10.0) resolve
    // Nager.PublicSuffix 3.8.0 (RuleProviders/Exceptions sub-namespaces, SimpleHttpRuleProvider).
#if CLUEDIN_V50
    private static readonly DomainParser domainParser = new(new SimpleHttpRuleProvider());
#else
    private static readonly DomainParser domainParser = new(new WebTldRuleProvider());
#endif

    public static bool TryParse(string domain, [NotNullWhen(true)]out DomainInfo? domainInfo)
    {
        try
        {
            domainInfo = domainParser.Parse(domain);
            return domainInfo != null;
        }
        catch (ParseException)
        {
            domainInfo = null;
            return false;
        }
    }
}
