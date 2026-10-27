# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.6.0"></a>
## [1.6.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.6.0) (2026-10-27)

### Features

* **Audio:** Retry dropped internet streams with exponential backoff ([02f5310](https://www.github.com/jeffu231/Radio-Automation/commit/02f5310074419b4051b94e1a69203a32143e7d67))

### Bug Fixes

* **audio:** Enumerate source sequences once ([b3d957b](https://www.github.com/jeffu231/Radio-Automation/commit/b3d957b34ba2684c3d12ad0dcc66bb72f68321c3))
* **audio:** Set Device.IsDefault from constructor and promote rule to error ([a87a4d1](https://www.github.com/jeffu231/Radio-Automation/commit/a87a4d1c79a6dbcc0700c0225c41ac885cd6e30c))
* **AudioTrackParser:** Match audio file extensions case-insensitively ([fd58ae0](https://www.github.com/jeffu231/Radio-Automation/commit/fd58ae0175e6e27508be7ec1b64e333ca238db68))
* **AudioTrackParser:** Return each track once from recursive scans ([66621c5](https://www.github.com/jeffu231/Radio-Automation/commit/66621c5fdbe38de3e3c5363b7789c7d846032e70))
* **AudioTrackParser:** Skip unreadable audio files in folder scans ([145c978](https://www.github.com/jeffu231/Radio-Automation/commit/145c978cb2b34bb4bee9976970e0937c1b98b8d8))
* **Extensions:** Support nullable enums in EnumerationExtension ([c9a88b6](https://www.github.com/jeffu231/Radio-Automation/commit/c9a88b64786ff8fd5e7cf7320cee110face6190a))
* **FileExtensions:** Detect encoding for short files and UTF-32 ([4bde539](https://www.github.com/jeffu231/Radio-Automation/commit/4bde539b23b9208fae460ad4e8a035051f602486))
* **globalization:** Format temperature file names invariantly ([158c6e7](https://www.github.com/jeffu231/Radio-Automation/commit/158c6e7a8401490c9596a77ad989d8f9a85ab25d))
* **globalization:** Parse PercentageConverter operands invariantly ([49faddf](https://www.github.com/jeffu231/Radio-Automation/commit/49faddf26432d57384866bd97e1a8c1993181d84))
* **lifetime:** Make Clock disposable and dispose it on close ([f688394](https://www.github.com/jeffu231/Radio-Automation/commit/f688394c57f9222704de90df0637176f50ae588d))
* **logging:** Replace console error with a real log entry ([4cf050f](https://www.github.com/jeffu231/Radio-Automation/commit/4cf050fc414f892cf74384378a0441e9c58bf3d1))
* **nullable:** Drop unscoped nullable annotations in MainWindowViewModel ([e0ec5a3](https://www.github.com/jeffu231/Radio-Automation/commit/e0ec5a30494a65b6280a486fbe061e207c84cdac))
* **Track:** Assign Id in the stream track constructor ([bca3841](https://www.github.com/jeffu231/Radio-Automation/commit/bca38410f0ede150375084c2ae888fb81f9df61f))
* **Track:** Handle empty lists in tag and genre formatting ([a495e7b](https://www.github.com/jeffu231/Radio-Automation/commit/a495e7b728823cf444e6077f1ece4bb9a7d468c2))

### Performance

* **analyzers:** Hoist constant arrays and cache JsonSerializerOptions ([da9b197](https://www.github.com/jeffu231/Radio-Automation/commit/da9b1978cf957afae79d71720662ca0f8ce4d45c))

### Continuous Integration

* **build:** Run unit tests before versioning ([d2483d8](https://www.github.com/jeffu231/Radio-Automation/commit/d2483d885e37e45c019bd05cc1d1aec0cc4489cd))
* **workflows:** Build the app project instead of the installer on PRs ([25a95dc](https://www.github.com/jeffu231/Radio-Automation/commit/25a95dca5126011ac3aa70a5cb57e98d9c0a99a1))
* **workflows:** Validate build and tests on pull requests ([63ef332](https://www.github.com/jeffu231/Radio-Automation/commit/63ef332cb4cd7a98b36457872867bf21192007cb))

### Documentation

* **CLAUDE:** Document test project and conventions ([c4ea68b](https://www.github.com/jeffu231/Radio-Automation/commit/c4ea68bb12ceb898eac70b50b5e719c06b3744dc))
* **CLAUDE:** Update known defect testing guidance ([dac7220](https://www.github.com/jeffu231/Radio-Automation/commit/dac72203000c95d50ca3d4d45adaf684e2a1e574))
* **readme:** Point to wiki for full documentation ([d141780](https://www.github.com/jeffu231/Radio-Automation/commit/d14178066891e49e1665065dcb6aa0df5abd4d8a))

<a name="1.5.2"></a>
## [1.5.2](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.5.2) (2026-10-23)

### Bug Fixes

* **Audio:** Recover playback after output device loss during RDP changes ([e8a84b1](https://www.github.com/jeffu231/Radio-Automation/commit/e8a84b11b8f77ce1d68f8f7a845a07f3db0dec4e))

### Continuous Integration

* **build:** Update GitHub Actions to current versions ([893f14d](https://www.github.com/jeffu231/Radio-Automation/commit/893f14d638e6ba19ee4ea275797a46931ded8b31))

<a name="1.5.1"></a>
## [1.5.1](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.5.1) (2026-04-16)

### Bug Fixes

* **Cleanup:** Address minor code quality issues ([650e9c0](https://www.github.com/jeffu231/Radio-Automation/commit/650e9c07b6ff48e0c5a714ea12be3af2b77dfd54))
* **Cleanup:** Address nullability issues in Audio Project ([497cfb9](https://www.github.com/jeffu231/Radio-Automation/commit/497cfb94090c7eb0359e73d5aa92ac4011ca48e2))
* **Validaton:** Add better preference page validation ([482cc83](https://www.github.com/jeffu231/Radio-Automation/commit/482cc83c3369ef7151b81cf36f50add2625474b7))
* **Volume:** Fix bug in setting initial audio volume at startup ([6f968e6](https://www.github.com/jeffu231/Radio-Automation/commit/6f968e63bb0e475ee9ce9f7dfc0945cf07a6509c))

### Documentation

* Remove old requirement doc ([736ef7f](https://www.github.com/jeffu231/Radio-Automation/commit/736ef7f8137419cae9069d57b1b491ca8c4d7b6d))

<a name="1.5.0"></a>
## [1.5.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.5.0) (2026-04-15)

### Features

* **Song publish:** Added ability to publish song info to an MQTT topic ([d66629a](https://www.github.com/jeffu231/Radio-Automation/commit/d66629a903581b2a6ea4ea3f1889c07dbad083b1))

### Documentation

* Add Claude file to describe the project ([3ae8422](https://www.github.com/jeffu231/Radio-Automation/commit/3ae842221f13c2f55ea21172eb23f6cbe63b0a80))

<a name="1.4.4"></a>
## [1.4.4](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.4.4) (2026-04-15)

### Continuous Integration

* Bump download action to latest ([8fa717c](https://www.github.com/jeffu231/Radio-Automation/commit/8fa717cdedbc0a0f494790d9b8c1470b50a65c3a))
* Update gitignore and workflow with latest actions and excludes ([2ceeb51](https://www.github.com/jeffu231/Radio-Automation/commit/2ceeb513746a9ebe9df7af6e0053569ca30f7310))

### Documentation

* Add CLAUDE.md with build commands and architecture overview ([b21799b](https://www.github.com/jeffu231/Radio-Automation/commit/b21799b5ad72b8ef2285fd138c6ad78c1e015bec))
* Fixed incorrect region name in Persistence service ([083fd2e](https://www.github.com/jeffu231/Radio-Automation/commit/083fd2e9860155c8201dbaf2bbb6ca4160f1db47))
* **README:** Update the readme with install information ([eecc554](https://www.github.com/jeffu231/Radio-Automation/commit/eecc554375a3acb3bf0cf4798be445009a4a4e4f))

<a name="1.4.3"></a>
## [1.4.3](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.4.3) (2026-03-04)

### Continuous Integration

* Remove click once publishing now that we have a proper msi ([8a54a38](https://www.github.com/jeffu231/Radio-Automation/commit/8a54a385055e74ea9e050e10dd8bfd49daa388cb))
* Specify version in winget releaser action ([6eb26f1](https://www.github.com/jeffu231/Radio-Automation/commit/6eb26f1c9083aca747e38f5d662684f37d1dff3e))

<a name="1.4.2"></a>
## [1.4.2](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.4.2) (2026-03-04)

### Bug Fixes

* Add a simple about dialog to show version ([233a663](https://www.github.com/jeffu231/Radio-Automation/commit/233a663f245198bbe796d30fa5475fec377a1642))
* Add link for help to github wiki ([6d9158c](https://www.github.com/jeffu231/Radio-Automation/commit/6d9158c0ebed90ace5dc3b1b78ebbf416793b2d6))
* Add logic to mark tracks not found invalid ([431a593](https://www.github.com/jeffu231/Radio-Automation/commit/431a5937b7b368fa10ae0978a82fea3344609c3a))
* Add playlist name to the title bar ([c7398c6](https://www.github.com/jeffu231/Radio-Automation/commit/c7398c67afa9e74b5b611dcf8afa3292f0ff2cf0))
* Fix add internet stream to have an input dialog ([69cc927](https://www.github.com/jeffu231/Radio-Automation/commit/69cc92731e25783a7f5043fd73568f37eb18c82c))
* Remove test code for default device ([c4209c9](https://www.github.com/jeffu231/Radio-Automation/commit/c4209c96e0778b66efc4673f893c35f3df815fc7))

### Continuous Integration

* Add action to update winget ([773e8ef](https://www.github.com/jeffu231/Radio-Automation/commit/773e8ef72d3c029297e1b593955a068131e8ed6b))
* Update to use global.json and bump action versions ([5a87522](https://www.github.com/jeffu231/Radio-Automation/commit/5a875227d82cd579f3253197420aa2df4c624997))

<a name="1.4.1"></a>
## [1.4.1](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.4.1) (2026-03-03)

### Bug Fixes

* Fix hard crash when no default audio devies are available ([2220af5](https://www.github.com/jeffu231/Radio-Automation/commit/2220af5afaf533d3e01305c8583dac887e6ce908))

<a name="1.4.0"></a>
## [1.4.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.4.0) (2026-02-28)

### Features

* **Serialization:** Remove Newtonsoft from Weather project ([66b6dee](https://www.github.com/jeffu231/Radio-Automation/commit/66b6deede32f99b111848319edd33d5928778c83))

### Bug Fixes

* **Serialization:** Clean up Newtonsoft redirects ([5d8202e](https://www.github.com/jeffu231/Radio-Automation/commit/5d8202e287c9db52eb8fd715421640da76b96450))

### Continuous Integration

* Remove pdb files from release builds ([7a1f39b](https://www.github.com/jeffu231/Radio-Automation/commit/7a1f39bd32068f26b9567f6d187ea565ca331afd))
* **Build:** Fix version passing ([7aa2d54](https://www.github.com/jeffu231/Radio-Automation/commit/7aa2d54b503e9fc25213b83c6318ce17ed592625))
* **Build:** Run versionize in bash shell ([92ea88f](https://www.github.com/jeffu231/Radio-Automation/commit/92ea88fb46f57581485fefc95142a564f386574b))
* **Build:** Switch to windows-latest ([1b8c8c4](https://www.github.com/jeffu231/Radio-Automation/commit/1b8c8c4780a446b8e385db300965e4466bcc569a))
* **Build:** Update build and output paths ([335f2c3](https://www.github.com/jeffu231/Radio-Automation/commit/335f2c3e35837fa87c1d9c8e407eada20a7100ea))
* **Build:** Use bash for changelog ([a3cb681](https://www.github.com/jeffu231/Radio-Automation/commit/a3cb681f583c2d5585debfe8582d3dd91f8871a9))
* **Installer:** Create a simple installer ([6ff1158](https://www.github.com/jeffu231/Radio-Automation/commit/6ff115858e090d633c4e15d51de9d39f6c615ca2))
* **Publish:** Create true single file publish config ([8087577](https://www.github.com/jeffu231/Radio-Automation/commit/80875772ab3a1a50fa861d567663cc5d900c0085))

<a name="1.3.0"></a>
## [1.3.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.3.0) (2026-02-28)

### Features

* **Serialization:** Remove Newtonsoft from Weather project ([66b6dee](https://www.github.com/jeffu231/Radio-Automation/commit/66b6deede32f99b111848319edd33d5928778c83))

### Bug Fixes

* **Serialization:** Clean up Newtonsoft redirects ([5d8202e](https://www.github.com/jeffu231/Radio-Automation/commit/5d8202e287c9db52eb8fd715421640da76b96450))

### Continuous Integration

* Remove pdb files from release builds ([7a1f39b](https://www.github.com/jeffu231/Radio-Automation/commit/7a1f39bd32068f26b9567f6d187ea565ca331afd))
* **Build:** Fix version passing ([7aa2d54](https://www.github.com/jeffu231/Radio-Automation/commit/7aa2d54b503e9fc25213b83c6318ce17ed592625))
* **Build:** Run versionize in bash shell ([92ea88f](https://www.github.com/jeffu231/Radio-Automation/commit/92ea88fb46f57581485fefc95142a564f386574b))
* **Build:** Switch to windows-latest ([1b8c8c4](https://www.github.com/jeffu231/Radio-Automation/commit/1b8c8c4780a446b8e385db300965e4466bcc569a))
* **Build:** Update build and output paths ([335f2c3](https://www.github.com/jeffu231/Radio-Automation/commit/335f2c3e35837fa87c1d9c8e407eada20a7100ea))
* **Installer:** Create a simple installer ([6ff1158](https://www.github.com/jeffu231/Radio-Automation/commit/6ff115858e090d633c4e15d51de9d39f6c615ca2))
* **Publish:** Create true single file publish config ([8087577](https://www.github.com/jeffu231/Radio-Automation/commit/80875772ab3a1a50fa861d567663cc5d900c0085))

<a name="1.2.4"></a>
## [1.2.4](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.2.4) (2026-02-28)

### Continuous Integration

* **Build:** Run versionize in bash shell ([92ea88f](https://www.github.com/jeffu231/Radio-Automation/commit/92ea88fb46f57581485fefc95142a564f386574b))

<a name="1.2.3"></a>
## [1.2.3](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.2.3) (2026-02-28)

### Continuous Integration

* **Build:** Fix version passing ([7aa2d54](https://www.github.com/jeffu231/Radio-Automation/commit/7aa2d54b503e9fc25213b83c6318ce17ed592625))

<a name="1.2.2"></a>
## [1.2.2](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.2.2) (2026-02-28)

<a name="1.2.1"></a>
## [1.2.1](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.2.1) (2026-02-28)

### Continuous Integration

* **Build:** Switch to windows-latest ([1b8c8c4](https://www.github.com/jeffu231/Radio-Automation/commit/1b8c8c4780a446b8e385db300965e4466bcc569a))

<a name="1.2.0"></a>
## [1.2.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.2.0) (2026-02-28)

### Features

* **Serialization:** Remove Newtonsoft from Weather project ([66b6dee](https://www.github.com/jeffu231/Radio-Automation/commit/66b6deede32f99b111848319edd33d5928778c83))

### Bug Fixes

* **Serialization:** Clean up Newtonsoft redirects ([5d8202e](https://www.github.com/jeffu231/Radio-Automation/commit/5d8202e287c9db52eb8fd715421640da76b96450))

### Continuous Integration

* Remove pdb files from release builds ([7a1f39b](https://www.github.com/jeffu231/Radio-Automation/commit/7a1f39bd32068f26b9567f6d187ea565ca331afd))
* **Build:** Update build and output paths ([335f2c3](https://www.github.com/jeffu231/Radio-Automation/commit/335f2c3e35837fa87c1d9c8e407eada20a7100ea))
* **Installer:** Create a simple installer ([6ff1158](https://www.github.com/jeffu231/Radio-Automation/commit/6ff115858e090d633c4e15d51de9d39f6c615ca2))
* **Publish:** Create true single file publish config ([8087577](https://www.github.com/jeffu231/Radio-Automation/commit/80875772ab3a1a50fa861d567663cc5d900c0085))

<a name="1.1.0"></a>
## [1.1.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.1.0) (2026-02-27)

### Features

* Preference for controlling shuffle when playlist rolls over ([bbd7db8](https://www.github.com/jeffu231/Radio-Automation/commit/bbd7db8f1afeca0bffc25a47f1d5fe9efa208fd2))

### Continuous Integration

* **Publish:** Bump Click Once Version ([c54ae80](https://www.github.com/jeffu231/Radio-Automation/commit/c54ae80b46e38babdeaed2772131eeed62897cc6))
* **Publish:** Click Once version Update ([82cac6f](https://www.github.com/jeffu231/Radio-Automation/commit/82cac6fe9093eb7d260a5d9b4935078b9d035127))

<a name="1.0.7"></a>
## [1.0.7](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.7) (2026-02-27)

### Bug Fixes

* Fix delay when closing the application ([7c9bf07](https://www.github.com/jeffu231/Radio-Automation/commit/7c9bf0777f663a71b668aade2cac664a34a7b272))
* **Cleanup:** Remove unecessary code ([f03ba9e](https://www.github.com/jeffu231/Radio-Automation/commit/f03ba9e825fe229ebe645b77db7bc82d05e59a0e))
* **Serialization:** Replace Catel Json Serializer with System.Text.Json ([e150a09](https://www.github.com/jeffu231/Radio-Automation/commit/e150a09272f1eac098c1e0c52c98cf9547fb38dd))

<a name="1.0.6"></a>
## [1.0.6](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.6) (2026-02-26)

### Bug Fixes

* Ensure playing and timers are stopped on form close ([cb8de1a](https://www.github.com/jeffu231/Radio-Automation/commit/cb8de1a21ea9ab80ee46f3db8e8d664f9dc4aa8e))
* Improper default audio device ([7ee733a](https://www.github.com/jeffu231/Radio-Automation/commit/7ee733af9cfa3576b285fbaf879ed4def761e62c))
* **Libraries:** Bump dependancies to latest versions ([a29e89e](https://www.github.com/jeffu231/Radio-Automation/commit/a29e89eacac4fc6647514d099ef64f7a96286358))
* **NET Version:** Bump to .NET 10 ([f1daab6](https://www.github.com/jeffu231/Radio-Automation/commit/f1daab6ecc2359cfe6f17ba1d150120745294528))

### Continuous Integration

* Fix build arguments and click once pub config ([0cec1bd](https://www.github.com/jeffu231/Radio-Automation/commit/0cec1bda4a19882e61505b69111b02bd1a9458c5))
* Fix incorrect workflow configuration ([84740fd](https://www.github.com/jeffu231/Radio-Automation/commit/84740fd3011c4fbab9a3604d795c3c20c4937a0f))
* update click once publish file ([b8b77d7](https://www.github.com/jeffu231/Radio-Automation/commit/b8b77d7358541fd3f4f58cbafd3b8afe03740751))
* Update workflow to ignore certain ci type files ([5cdb1a0](https://www.github.com/jeffu231/Radio-Automation/commit/5cdb1a06ef2ee0ca46e9694eb50302314924abb6))
* **versioning:** Add versionize config ([5fad7e4](https://www.github.com/jeffu231/Radio-Automation/commit/5fad7e41b681ec1d4922aee9da19708ae4604a55))

### Documentation

* Add badge logos ([ea0609c](https://www.github.com/jeffu231/Radio-Automation/commit/ea0609c5098f796475420e6af84b788011e7ffb6))
* Add badges ([c32b14a](https://www.github.com/jeffu231/Radio-Automation/commit/c32b14af64a33bbaf8a919113cdfde20a5fc50f4))
* Fix badges links ([493be49](https://www.github.com/jeffu231/Radio-Automation/commit/493be4971a655754227a9793a62177e8de6f4487))
* Update README.md with better docs ([4685a63](https://www.github.com/jeffu231/Radio-Automation/commit/4685a63cd64bdbf2692d22f421b92bbb2ec3b2c4))

<a name="1.0.5"></a>
## [1.0.5](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.5) (2025-12-29)

### Bug Fixes

* Add logic to handle current song file in use without crashing ([2653cc8](https://www.github.com/jeffu231/Radio-Automation/commit/2653cc81879d72aa349f64003ca56324151184d1))

<a name="1.0.4"></a>
## [1.0.4](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.4) (2025-11-14)

<a name="1.0.3"></a>
## [1.0.3](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.3) (2025-11-14)

### Bug Fixes

* remove unused Schedule project ([810f057](https://www.github.com/jeffu231/Radio-Automation/commit/810f0574552664465687ecc11b45dc627d391d01))

<a name="1.0.2"></a>
## [1.0.2](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.2) (2025-11-14)

<a name="1.0.1"></a>
## [1.0.1](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.1) (2025-11-14)

<a name="1.0.0"></a>
## [1.0.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.0) (2025-11-14)

### Bug Fixes

* Clean up Weather project ([7a4cef8](https://www.github.com/jeffu231/Radio-Automation/commit/7a4cef892846d7e5b9245b5c30b8b15ffb8dbf6e))
* Convert mono streams to stereo ([8264b19](https://www.github.com/jeffu231/Radio-Automation/commit/8264b19f5464065f422471a12b63a59e9fd3e1db))
* Remove unused NCrontab libraries ([ad23c54](https://www.github.com/jeffu231/Radio-Automation/commit/ad23c542094fb946a55eeeaaa4e5b274d22c0e38))
* set right audio volume to left for single channel sources ([226b172](https://www.github.com/jeffu231/Radio-Automation/commit/226b17263f491950bd067fcee8dd11e0a57b2514))

<a name="1.0.0"></a>
## [1.0.0](https://www.github.com/jeffu231/Radio-Automation/releases/tag/v1.0.0) (2025-11-14)

### Bug Fixes

* Clean up Weather project ([7a4cef8](https://www.github.com/jeffu231/Radio-Automation/commit/7a4cef892846d7e5b9245b5c30b8b15ffb8dbf6e))
* Convert mono streams to stereo ([8264b19](https://www.github.com/jeffu231/Radio-Automation/commit/8264b19f5464065f422471a12b63a59e9fd3e1db))
* Remove unused NCrontab libraries ([ad23c54](https://www.github.com/jeffu231/Radio-Automation/commit/ad23c542094fb946a55eeeaaa4e5b274d22c0e38))
* set right audio volume to left for single channel sources ([226b172](https://www.github.com/jeffu231/Radio-Automation/commit/226b17263f491950bd067fcee8dd11e0a57b2514))

