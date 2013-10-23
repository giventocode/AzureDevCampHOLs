# Windows Azure Web Sites and Virtual Machines using ASP.NET and SQL Server - for Visual Studio 2012#

## Hands-on Lab ##

### Introduction ###

Hands-on labs are sets of step-by-step guides that are designed to help you learn how to use key Windows Azure services and features.  Each Lab provides instructions to guide you through the process of developing a complete application.

In this lab, you will learn how to create ASP.NET web applications that connect to virtual machines running in Windows Azure. First, you will create a virtual machine with SQL Server 2012 installed using the Windows Azure Management Portal and configure it to allow external connections. Then, you will create a simple ASP.NET MVC 4 web application using Entity Framework that accesses the database in the SQL Server virtual machine. The web application will take advantage of Full-Text Search features in SQL Server 2012 to search for contacts data.  You will complete the lab by deploying the application to Windows Azure Web Sites using Visual Studio.

> **Note:** You can download the latest build of the Windows Azure Training Kit which includes a tested version of this HOL from here: http://bit.ly/WindowsAzureTK.

Visit our [GitHub Homepage](http://windowsazure-trainingkit.github.com/) for more information about the **Windows Azure Training Kit**.

### Repository Structure ###

In the **root** folder of this repository you will find the hands-on lab document, **HOL.md**. Before beginning with the lab exercises, make sure you have followed all the required steps indicated at the setup section of the document.

In the **Source** folder you will find the source code of each of the exercises, as well as the assets and setup scripts. Throughout the lab you will be instructed to open and explore the different solutions from the source folder. It is typically comprised of the following subfolders:

- **Assets:** This folder contains files that are used throughout the exercises.
- **_Exercise Name_:** Each exercise that requires a programming solution has its own code folder.
  - **Begin:** The begin solution is the initial incomplete solution that you will finish by following the steps of the corresponding exercise.
  - **End:** The end solution is the final result you will achieve at the end of an exercise.
- **Setup:** This folder contains the dependency files and the setup scripts necessary to initialize specific configurations of the lab, being its execution is required in the majority of the Hands-on Labs.

### Get Started ###

In order to run the solutions of the exercises provided by this lab you will first need configure your environment and install any necessary prerequisites such as runtimes, components, or libraries. For your ease, you can download and run the dependency checker [here] (http://contentinstaller.blob.core.windows.net/dependency-checker/DC.exe) to automatically check and install all the requirements.  Each lab also includes setup instructions for getting started.

### Contributing to the Repository ###

If you find any issues or opportunties for improving this hands-on lab, fix them! Feel free to contribute to this project by [forking](http://help.github.com/fork-a-repo/) this repository and make changes to the content. Once you've made your changes, share them back with the community by sending a pull request. Please see GitHub section [How to send pull requests](http://help.github.com/send-pull-requests/) and the [Windows Azure Contribution Guidelines](http://windowsazure.github.com/guidelines.html) for more information about contributing to projects.

### Reporting Issues ###

If you find any issues with this hands-on lab that you can't fix, feel free to report them in the [issues](https://github.com/WindowsAzure-TrainingKit/HOL-ASPNETAzureWebSitesVMSQL-VS2012/issues) section of this repository.