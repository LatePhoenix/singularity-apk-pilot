Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- .cursor/rules/packaging-rules.mdc



Task:

Create Windows packaging support.



Create:

\- build/packaging/InnoSetup/Installer.iss

\- build/packaging/scripts/pack.ps1

\- build/packaging/scripts/bundle-payload.ps1

\- payload layout for adb tools and app manifest/apk



Requirements:

\- Install the WPF app.

\- Bundle portable adb in a predictable relative path.

\- Bundle the current app manifest and apk payload.

\- Add optional post-install launch checkbox using the correct Inno Setup \[Run] section behavior.

\- Keep paths relative and portable.

\- Do not implement code signing yet; leave clear TODO markers.



After changes:

\- Summarize produced packaging artifacts.

\- Explain how to run the packaging script.

\- Note manual prerequisites like Inno Setup installation if needed.

