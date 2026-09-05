# Workspace Guidelines & Rules

## Mod Packaging & Deployment Rules

1. **禁止向游戏目录部署包**：
   - 严禁向系统 `Documents/Klei` 游戏相关目录（`Documents/Klei/OxygenNotIncluded/mods/local/`）部署、覆盖或复制任何模组文件及解压包。

2. **允许在模组开发根目录部署模组文件夹**：
   - 允许在模组开发根目录（`Automatic Industry` 工作区）生成/解压发布版的模组文件夹（例如 `AutomaticIndustry-2.4.26/`、`ModMenu-1.4.7/`）及对应的 `.zip`、`-src-*.zip` 打包产物。

3. **版本构建与测试完整性**：
   - 其余更新与测试规则保持一致：每次发布前须通过完整编译（0 Error/Warning）、多语言一致性检查及 Sandbox 自动化测试套件。
