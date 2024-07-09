# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.3]

### Fixed
- Fixed issue with 2.0.2 causing enemies to be invisible
- Fixed issue with Lethal Jumpany main menu being a pixel off vertically

## [2.0.2]

### Fixed
- Fixed issue with PintoBoy screens overlapping
- PintoBoy camera can only see PintoBoy assets now. This should fix the bugs with seeing ship UI in the PintoBoy's screen

## [2.0.0]

### Additions
- added new game "Facility Dash"
    - fight off and hide from monsters by tapping or holding the button in this accurate simulation of what its like to fend off monsters in the facilities.
    - it is random what game a PintoBoy will spawn with
- added 9 new colors of PintoBoy
    - White, Charcoal, Red, Peach, Banana, Mint, Teal, Turquoise, Sky Blue, Fairy
- added ability to hold the PintoBoy's button
    - Facility Dash: holding makes the player hide
    - Lethal Jumpany: holding longer makes the player jump higher
- revamped the PintoBoy model
    - the polygon count is way lower while keeping the same aesthetic
    - added new Select/Start buttons and speaker grill textures to make it look less plain
- changed screen resolution of the PintoBoy from 160x160 to 120x120
    - this is to help visual clarity of the PintoBoy screen while it is in your hand
    - adjusted Lethal Jumpany to account for this while keeping the feel of how it originally played
- Lethal Jumpany: There is now a chance a spider will spawn right after a lootbug when your score is higher
- the sprite palette was slightly altered

### Performance changes
- lowered the resolution of the PintoBoy item icon. It did not need to be 1080x1080 lol
- more than halved the polygon count of the PintoBoy model. less useless detail while still keeping the same aesthetic
- lowered the resolution of the PintoBoy model textures while also adding an additional speaker texture to make it look nicer

### Fixes
- fixed the PintoBoy having a weight of -82lbs. It now weighs 10lbs (Thanks @nyctodarkmatter for pointing it out)
- fixed the battery of the PintoBoy not working correctly (Thanks @brawleyiscringe for pointing it out)
- set some scannode properties to their correct values
- fixed the price of the PintoBoy
- fixed the screens of PintoBoys not despawning when leaving those PintoBoys on a planet
    - this would cause insane performance dips if many PintoBoys were spawned in previous rounds and the lobby wasn't reset

### Credits
Thank you to Shiru/shiru8bit for the sounds and music in this mod! I used more of their work in Facility Dash. <br>
Check them out here!: https://opengameart.org/users/shiru8bit

I spent forever debugging and making sure there weren't any glaring problems with this release. Hopefully that is true. If you want to report anything to me, let me know here: 
<br>[Link](https://discord.com/channels/1168655651455639582/1187518133066551326)

## [1.0.4]

### Changed
- updated LethalLib to 0.13.2. This should fix compatibility with modded moons

## [1.0.3]

### Fixed
- fixed issue with PintoBoys not spawning correctly for non-host players
- Added config for changing the PintoBoy rarity

## [1.0.1]

### Fixed
- fixed issue with non-host players not being able to play

## [1.0.0]
- Initial Release