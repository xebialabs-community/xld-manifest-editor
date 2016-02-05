Manifest Editor for XL Deploy
=============================

XL Deploy Manifest Editor auxiliary program, which helps in putting together a valid deployit-manifest.xml file.
For more information about the manifest editor, see XL Deploy Manifest Editor Manual https://docs.xebialabs.com/xl-deploy-tfs-plugin/4.5.x/manifestEditorManual.html

Requirements
-------------
The XL Deploy Manifest Editor is a desktop application written in .NET 4.5, so it should work out-of-the box in Windows environments that have this version installed. Other environments need to use an emulator that supports WPF (Windows Presentation Foundation) applications.

For more information about the XL Deploy manifest format, see the XL Deploy Packaging Manual http://docs.xebialabs.com/releases/latest/deployit/packagingmanual.html.

For general information about XL Deploy and CI types, see the XL Deploy Reference Manual.
http://docs.xebialabs.com/releases/latest/deployit/referencemanual.html

Creating a distribution
------------------------
Use './gradlew clean build' will create a new distribution for manifest editor in build/distributions directory  
