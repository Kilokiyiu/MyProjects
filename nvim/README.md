# Neovim 个人配置

一套基于 [lazy.nvim](https://github.com/folke/lazy.nvim) 的 Neovim 个人配置，注重现代化编辑体验与简洁结构。

---

## 项目结构

```
nvim/
├── init.lua                  # 入口文件
├── lazy-lock.json            # 插件版本锁定
├── lua/
│   ├── core/
│   │   └── options.lua       # Neovim 基础选项配置
│   ├── config/
│   │   └── lazy.lua          # lazy.nvim 插件管理器初始化
│   └── plugins/              # 插件配置目录（lazy.nvim 自动加载）
│       ├── colorscheme.lua   # 主题
│       ├── completion.lua    # 自动补全
│       ├── lsp.lua           # LSP 语言服务器
│       ├── treesitter.lua    # 语法树高亮
│       └── ui.lua            # UI 美化
```

---

## 基础配置

| 选项 | 设置 |
|------|------|
| Leader 键 | `<Space>` |
| 行号 | 相对行号 + 绝对行号 |
| 缩进 | 2 空格，自动展开 Tab |
| 光标行 | 启用 |
| 鼠标 | 全模式支持 |
| 搜索 | 忽略大小写，智能大小写 |
| 分屏 | 新窗口默认在右侧和下方 |
| 文件类型 | `.cs` 映射为 `c_sharp` |

---

## 插件列表

### 外观主题

| 插件 | 说明 |
|------|------|
| [catppuccin](https://github.com/catppuccin/nvim) | 主題配色方案 |

### UI 增强

| 插件 | 说明 |
|------|------|
| [lualine.nvim](https://github.com/nvim-lualine/lualine.nvim) | 底部状态栏 |
| [barbar.nvim](https://github.com/romgrk/barbar.nvim) | 顶部标签页 |
| [nvim-tree.lua](https://github.com/nvim-tree/nvim-tree.lua) | 侧边文件树 |
| [rainbow-delimiters.nvim](https://github.com/HiPhish/rainbow-delimiters.nvim) | 彩虹括号匹配 |
| [noice.nvim](https://github.com/folke/noice.nvim) | 消息/命令行/弹出菜单美化 |
| [nvim-notify](https://github.com/rcarriga/nvim-notify) | 通知弹窗（noice 依赖） |
| [nui.nvim](https://github.com/MunifTanjim/nui.nvim) | UI 组件库（noice 依赖） |
| [nvim-web-devicons](https://github.com/nvim-tree/nvim-web-devicons) | 文件类型图标 |

### 代码补全

| 插件 | 说明 |
|------|------|
| [blink.cmp](https://github.com/Saghen/blink.cmp) | 高性能补全引擎 |
| [blink-copilot](https://github.com/fang2hou/blink-copilot) | GitHub Copilot 集成 |
| [lspkind.nvim](https://github.com/onsails/lspkind.nvim) | 补全项类型图标 |
| [lazydev.nvim](https://github.com/folke/lazydev.nvim) | Neovim Lua 开发增强 |

**补全来源优先级**：`copilot` > `lazydev` > `path` > `snippets` > `lsp` > `buffer`

**特色功能**：
- 注释区域内仅使用 `buffer` 补全
- 支持 Ghost Text（虚影文本预览）
- 自动括号插入
- 签名帮助自动弹出

### LSP 语言服务器

| 插件 | 说明 |
|------|------|
| [mason.nvim](https://github.com/mason-org/mason.nvim) | LSP/DAP/Linter 管理器 |
| [nvim-lspconfig](https://github.com/neovim/nvim-lspconfig) | LSP 客户端配置 |

**当前配置的语言服务器**：`lua-language-server`

### 语法高亮

| 插件 | 说明 |
|------|------|
| [nvim-treesitter](https://github.com/nvim-treesitter/nvim-treesitter) | 语法树解析与高亮 |

**支持语言**：`vim`, `c`, `lua`, `vimdoc`, `query`, `javascript`, `python`, `c_sharp`, `cpp`, `css`, `html`, `markdown`, `json`

---

## 快捷键说明

### 补全菜单

| 按键 | 功能 |
|------|------|
| `<A-j>` / `<C-n>` | 选择下一项 |
| `<A-k>` / `<C-p>` | 选择上一项 |
| `<Tab>` / `<CR>` | 确认补全 |
| `<A-/>` | 显示/隐藏补全菜单 |
| `<A-n>` / `<A-p>` | 仅使用 buffer 补全 |
| `<C-u>` / `<C-d>` | 补全文档上下滚动 |

---

## 环境要求

- Neovim >= 0.10
- Git
- 可选：[Nerd Font](https://www.nerdfonts.com/)（用于显示图标）

---

## 安装方式

1. 备份原有配置：
   ```bash
   mv ~/.config/nvim ~/.config/nvim.bak
   ```

2. 克隆本仓库：
   ```bash
   git clone <仓库地址> ~/.config/nvim
   ```

3. 首次启动 Neovim 时，lazy.nvim 会自动下载并安装所有插件。

---

## 网络加速

配置中已内置代理加速插件下载（针对 GitHub）：

```lua
git = {
  url_format = "https://github.com/%s.git",
  proxy = "http://127.0.0.1:38457",
}
```

如需修改代理地址，请编辑 `lua/config/lazy.lua`。
