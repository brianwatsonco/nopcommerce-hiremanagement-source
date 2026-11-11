# nopCommerce HireManagement Plugin (Source)
Source repository for the `Nop.Plugin.Misc.HireManagement` plugin for **nopCommerce 5.00**.

## Repository
- Source repo: `https://github.com/brianwatsonco/nopcommerce-hiremanagement-source.git`
- Primary development branch: `feature/hiremanagement-v5`

## Target Platform
- nopCommerce: `5.00.x`
- .NET: `10.0`

## Plugin Purpose
The HireManagement plugin adds rental-focused capabilities to nopCommerce, including:
- Admin diary for booking/unavailability management
- Quantity-aware availability states
- Public rental date/time selection with availability visualization
- Order-to-booking synchronization and cancellation release handling
- Plugin-owned rental product template support

## Important Setup
- Assign products that should use hire/rental behavior to the **Rental product** template provided by this plugin (`ProductTemplate.Rental.cshtml`).
- If a product does not use the Rental template, the rental date/time UI and availability workflow will not be applied.
- For rental products managed by this plugin, set **Inventory method** to **Do not track inventory** in nopCommerce Product Admin. Availability is managed by booking date windows and rental quantity over time; core stock decrements can conflict with booking-window availability.

## Build
From repository root:

This standalone plugin project references nopCommerce/FluentMigrator assemblies for compile-time only.
Those references are configured with `Private=false` and `CopyLocalLockFileAssemblies=false`, so they are **not** copied into the plugin output/package.

## Output
Build output should be copied to (or produced at):

- `Presentation\Nop.Web\Plugins\Misc.HireManagement`

## Package (ZIP)
Example packaging flow:
$plugin="Misc.HireManagement" $out=".\Presentation\Nop.Web\Plugins$plugin" $zip="._plugin_packages$plugin-$((Get-Date).ToString('yyyyMMdd-HHmmss')).zip"
if (Test-Path $zip) { Remove-Item $zip -Force } New-Item -ItemType Directory -Force (Split-Path $zip) | Out-Null Compress-Archive -Path "$out*" -DestinationPath $zip -CompressionLevel Optimal Write-Host "Created: $zip"


## Development Notes
- Keep plugin behavior self-contained in the plugin.
- Prefer to use standalone JS/CSS files over inline script/style.
- Include migration updates for schema/localization changes.

## Phase 2 (v5.00.2) Updates
- Booking identity now supports `OrderItemId` (with migration), and order sync prefers order-item matching.
- Admin save flow now allows conflicting records but shows a warning instead of hard-blocking.
- Website booking sync now uses authoritative validated save logic and transaction-protected persistence.
- Public availability endpoints now enforce stricter year/month/duration bounds and use duration-aware monthly availability.
- Server-side cart/checkout enforcement added via shopping cart service override to prevent bypassing availability checks.

## Phase 3 (v5.00.3) Updates
- Rental price refresh now updates consistently when any product option changes (days, mileage, extras, and date/time selectors).
- Server and client pricing logic aligned so cart totals match displayed totals for rental selections.
- Added cart overlap guard to block overlapping rental lines for the same product when capacity would be exceeded.
- Public availability calendar now renders day-by-day occupancy (`durationDays=1`) for clearer diary parity.
- Calendar day-state styling updated so FullyBooked / Unavailable / LowAvailability states are visually distinct in the storefront datepicker.
- Removed stock-quantity hard block in public rental validation; availability is now driven by booking-window logic.
- Availability calculations now apply an effective rental capacity floor to avoid month-wide false `0/0` FullyBooked rendering after core stock side effects.
- Added setup recommendation to use **Do not track inventory** for rental products managed by this plugin.

## Contributing
Issues and pull requests are welcome.  
Please include:
- clear reproduction steps
- expected vs actual behavior
- nopCommerce version and plugin version
- screenshots/logs where relevant

##PolyForm Noncommercial License 1.0.0

Copyright (c) 2026 Brian Watson & Co.

Licensed under the PolyForm Noncommercial License 1.0.0 (the “License”); you may not use this file except in compliance with the License.
You may obtain a copy of the License at:

https://polyformproject.org/licenses/noncommercial/1.0.0 
