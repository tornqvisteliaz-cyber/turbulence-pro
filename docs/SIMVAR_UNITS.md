# SimVar units used by Milestone 1

Checked against the MSFS 2024 retail SimVar pages. The unit string is what `SimConnect_AddToDataDefinition` requests.

| SimVar | Requested unit | Meaning used |
| --- | --- | --- |
| PLANE ALTITUDE | feet | Aircraft altitude |
| PLANE ALT ABOVE GROUND | feet | Height above surface, including obstacles |
| AIRSPEED INDICATED | knots | IAS |
| GROUND VELOCITY | knots | Ground speed |
| VERTICAL SPEED | feet per second | Indicated vertical speed. The 2024 page says feet per second, not feet per minute. UI multiplies by 60 for fpm. |
| PLANE PITCH DEGREES | radians | Pitch. The name says degrees. The SDK unit is radians. |
| PLANE BANK DEGREES | radians | Bank. The name says degrees. The SDK unit is radians. |
| PLANE HEADING DEGREES TRUE | radians | True heading. The name says degrees. The SDK unit is radians. |
| ACCELERATION BODY X | feet per second squared | Body lateral axis. The page also says "east/west". That world wording is not used. Not G-load. |
| ACCELERATION BODY Y | feet per second squared | Body vertical axis. Not total G-load. High-pass only, so a steady value near 32 ft/s^2 does not count. |
| ACCELERATION BODY Z | feet per second squared | Body longitudinal axis. The page also says "north/south". That world wording is not used. |
| AMBIENT WIND X | meters per second | East/west |
| AMBIENT WIND Y | meters per second | Vertical |
| AMBIENT WIND Z | meters per second | North/south |
| AMBIENT WIND VELOCITY | knots | Wind speed |
| AMBIENT WIND DIRECTION | degrees | True north |
| AIRCRAFT WIND X | knots | Lateral axis. Rudder assist can force this to 0 on takeoff. |
| AIRCRAFT WIND Y | knots | Vertical axis |
| AIRCRAFT WIND Z | knots | Longitudinal axis |
| TOTAL WEIGHT | pounds | Requested in pounds. Confirm on an odd title with SimVar Watcher. |
| SIM ON GROUND | bool | On-ground flag |
| AMBIENT IN CLOUD | bool | In cloud. Not a cloud geometry sample. |
| AMBIENT PRECIP STATE | mask | 2 none, 4 rain, 8 snow |
| AMBIENT PRECIP RATE | millimeters | Precipitation rate |
| TITLE | string128 | aircraft.cfg title |

`SEMIBODY LOADFACTOR Y` is the load factor (acceleration on Y divided by gravity). It is not subscribed in this milestone, so body acceleration is not mislabelled as G.

Camera and wake are interfaces only. No camera SimVars are written.
