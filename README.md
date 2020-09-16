# Phonics Example

This is an example project for the PxPre Phonics library.

The project uses the Phonics library as a submodule. See https://github.com/Reavenk/PxPre-Phonics for more information.

For more information, see the README.md in the Phonics submodule.



# The Demo

The example comes with a Unity scene called Sample. It uses the IMGUI so it must be played in order to see anything.

All sample logic is in the Sample.cs script, which is assigned to the "Main" GameObject in Sample.unity.

It comes with a file that has some example instruments. This is the same example file that comes with the [Precision Keyboard](https://play.google.com/store/apps/details?id=tech.pixelprecision.precisionkeyboard) application, which uses this library. The application is also an editor. A web build that supports uploading and downloading can be found [here](https://pixeleuphoria.com/blog/index.php/demo-webkeys/).

Below is the list of the different UI elements in the example.

## Master
The master volume.

## E-Stop
The button will turn red if any notes are playing. Press when red to fully stop all audio.

## Keyboard
A sample keyboard on the 4th octave.

## Instrument Selection
A list of instruments to select from. The selected instrument will be what is played on the keyboard.