return {
  --下文是treesitter语法树的配置，主要用于实现语法高光
  "nvim-treesitter/nvim-treesitter",
  lazy = false,
  build = ":TSUpdate",

  config = function()
    local treesitter = require("nvim-treesitter")
    treesitter.setup({

      --打开高光
      highlight = {
        enable = true
      },

      --智能缩进
      indent = {
        enable = true
      },

      --自动更新
      auto_install = true,
    })

    --下载解析器需要解析的语言
    treesitter.install{
      "vim", "c", "lua", "vimdoc", "query",
      "javascript", "python", "c_sharp", "cpp", "css", "html",
      "markdown", "json"
    }

    --设置需要高光的语法，需要在pattern加入需要高光的语法，否则会报错
    vim.api.nvim_create_autocmd("FileType", {
      pattern = {
        "vim", "c", "lua", "vimdoc", "query",
        "javascript", "python", "c_sharp", "cpp", "css", "html",
        "markdown", "json"
      },
      callback = function()
        vim.treesitter.start()
      end,
    })

  end
}
