\# Master Implementation Plan



\## Objective

Build a Windows WPF guided installer for non-technical users to install APKs on Meta Quest 2/3 and Android phones.



\## Product constraints

\- Quest-first UX, Android second.

\- Single-purpose guided wizard, not a general adb toolbox.

\- Deterministic recovery for common install failures.

\- Reusable app manifest model.

\- Windows packaging with Inno Setup.



\## Architecture

\- Installer.App

\- Installer.Core

\- Installer.Infrastructure

\- Installer.Contracts



\## Work order sequence

1\. Solution skeleton and project wiring

2\. adb core abstractions and process execution

3\. Device detection and classification

4\. Install planning and execution

5\. Wizard state machine and flow strategies

6\. WPF shell and step pages

7\. Error classification and auto-fix actions

8\. Diagnostics export

9\. Packaging and installer



\## Acceptance bar

\- Solution builds cleanly.

\- Core parser/decision logic covered by unit tests.

\- Manual test path documented for Quest unauthorized -> authorized -> install.

\- Manual test path documented for Android phone unauthorized -> authorized -> install.

\- Installer package launches app post-install.

