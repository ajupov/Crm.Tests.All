# Crm.Tests.All

API tests for [Lite CRM](https://litecrm.org)

## Usage
You must use account for `Resource Owner Password Credentials` flow.

For the local running tests you should create `appsettings.local.json` file in the root of the repository with content:
```
{
  "HostsSettings": {
    "ApiHost": "http://localhost:9000",
    "OAuthHost": "http://localhost:3000"
  },
  "OAuthSettings": {
    "ClientId": "site-local",
    "Username": "", // From account
    "Password": ""  // From account
  }
}
```

## Development
1. Clone this repository
2. Switch to a `new branch`
3. Make changes into `new branch`
4. Create pull request from `new branch` to `master` branch
5. Require code review
6. Merge pull request after approving
