If you are simply looking for assets from a specific minigame, the folder you are looking for is:
Assets/Bundled/Games/(minigame name)/Sounds

If you would like to contribute a minigame, and are using this folder for development purposes, please make sure your folder name is formatted like so:
Assets/Resources/Sfx/games/minigameNameInCamelCase

When you are finished developing your minigame, you may use the Game Bundler tool to automatically add your minigame to an Asset Bundle. In the Unity Editor, go to Tools > Game Bundler. 
Make sure you fill out the Asset Bundle metadata in the Minigame constructor when doing so -- you can copy this setup from any other minigame's script.