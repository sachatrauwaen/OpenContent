# OpenContent

Structured Content editing for DNN (Dotnetnuke)

*Goals*

* Html module remplacement for responsive websites
* Easy content editing of complex layouts by end users
* Content editing for websites using frameworks like bootstrap

<img src="https://cloud.githubusercontent.com/assets/5989191/20521415/79c19126-b0ab-11e6-9cae-f33ae35e554a.jpg" width="45%"></img> <img src="https://cloud.githubusercontent.com/assets/5989191/20521433/90932b76-b0ab-11e6-8702-6c2656b89b89.JPG" width="45%"></img> 

*Features*

* Structured content editing of complex data (from single item to multiple lists)
* Template based rendering
* Multi language
* Template exchange with data definition and templates
* Module title editing from the Content editing UI
* Online template editing


Documentation : [url:https://opencontent.readme.io]

Demos : [url:http://www.openextensions.net/dnn-modules/opencontent/bootstrap]

Templates download : [url:http://www.openextensions.net/templates/open-content]



[![Join the chat at https://gitter.im/sachatrauwaen/OpenContent](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/sachatrauwaen/OpenContent?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build by AppVeyor](https://ci.appveyor.com/api/projects/status/github/sachatrauwaen/OpenContent?branch=master&svg=true)](https://ci.appveyor.com/project/sachatrauwaen/opencontent/)
https://ci.appveyor.com/project/sachatrauwaen/opencontent


### Contributions

The Master branch only holds the latest stable version of the product. Navigate to dev branch in order to see latest work.

Create a feature branch (branched from develop branch) for every feature you want to contribute.

Before sending the Pull Requests, make sure you have the latest develop branch merged into you feature branch.
Pull Requests should be targeted to develop branch, not master! 

To quickly create a feature branch based on develop; git checkout -b my_contribution develop


### Set up development environment

To set up a development environment and build DNN Dev Tools, just follow the steps below:

* Download and extract the source code or clone the Git repository
* Install a new DNN instance (DNN 07.03.00 or later) under \PATH\TO\OpenContent\..\Website
* Open the solution file \PATH\OpenContent\OpenContent.sln in Visual Studio 2015 (launch Visual Studio as administrator)
* Build in Release mode, which creates the DNN module zip file \PATH\TO\OpenContent\..\Website\Install\Module\OpenContent_[XX.XX.XX]_Install.zip
* Log-in as host administrator and browser to the page "Host > Extensions" and install DNN Dev Tools under "Available Extensions > Modules"
* (optionally) Build the solution in Debug mode to copy the files to the Website folder instead of creating a DNN module zip file
* (optionally) Open a Powershell console and run \PATH\TO\OpenContent\AutoDeployment.ps1 to automatically copy frontend resources (like scripts or styles) to the Website folder when changing them in the solution
