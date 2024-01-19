using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

public class MyClaimsPrincipal : ClaimsPrincipal
{
    public IDictionary<string, string> claims { get; private set; }
    public ClaimsPrincipal current { get; private set; }
    public IIdentity identity { get { return base.Identity; } }

    public MyClaimsPrincipal() : base()
    {
        this.claims = new Dictionary<string, string>();
        this.current = ClaimsPrincipal.Current; // May not be needed if you always construct with claims
    }

    // You can construct a MyClaimsPrincipal with an existing list of claims
    public MyClaimsPrincipal(IEnumerable<Claim> claims) : base(new ClaimsIdentity(claims))
    {
        this.claims = new Dictionary<string, string>();
        foreach (var claim in claims)
        {
            this.claims[claim.Type] = claim.Value;
        }
        this.current = this; // Current is this instance itself
    }

    public override bool IsInRole(string role)
    {
        return this.HasClaim(ClaimTypes.Role, role);
    }

    // Method to add a new claim
    public void AddClaim(string type, string value)
    {
        var claim = new Claim(type, value);
        ((ClaimsIdentity)this.Identity).AddClaim(claim);
        this.claims[type] = value; // Update the dictionary with the new claim
    }
}
