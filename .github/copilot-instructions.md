# Copilot Instructions

## Project Guidelines
- For nopCommerce plugin project references, prefer linking Nop.Web.Framework.csproj and Nop.Web.csproj rather than multiple lower-level library project references when using source-based references.
- Use release source path H:\Brian\Visual Studio\nopCommerce_5.00.0_Source (not beta path) for nopCommerce 5 reference checks.
- Design a new nopCommerce 5-compliant theme from scratch instead of upgrading the legacy McRent/CarHaven-derived theme.

## Rental Page Behavior
- Ensure the price refreshes when any configurable option changes, and that cart/server pricing matches the displayed total.

## Debugging Guidelines
- During troubleshooting/testing, disable JS and CSS minification to simplify debugging and verify runtime behavior.