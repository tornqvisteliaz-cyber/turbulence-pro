# Turbulence Pro — Milestone 1

External MSFS 2024 companion. This milestone is the core engine, not camera control and not wake.

Pipeline: sample, measured detector, estimator, procedural gust field, realism gate, effect mixer, procedural audio, UI.

Measured turbulence is the high-pass change in body acceleration, vertical speed, wind, and attitude. Body acceleration is feet per second squared, not G-load.

Simulated turbulence is the procedural field. The UI shows both. Reason codes include `simulated:statistical-model`. CAT is statistical, not a forecast.

See BUILD.md and docs/SIMVAR_UNITS.md.
