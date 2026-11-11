# Nop.Plugin.Misc.HireManagement

## Important recommendation for rental products

For products managed by this plugin, set **Inventory method** to **Do not track inventory** in nopCommerce Product Admin.

This plugin manages hire availability by booking date windows and rental quantity over time. If core inventory tracking is enabled for rental products, stock decrements can conflict with booking-window availability and produce incorrect availability behavior.
