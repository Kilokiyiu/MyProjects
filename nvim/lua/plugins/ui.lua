return{
  {
    --底部状态栏
    "nvim-lualine/lualine.nvim",
    dependencies = {
      "nvim-tree/nvim-web-devicons"
    },
    opts = {}
  },

  {
    --顶部状态栏
    "romgrk/barbar.nvim",
    version = "^1.0.0",
    dependencies = {
      "lewis6991/gitsigns.nvim",
      "nvim-tree/nvim-web-devicons",
    },
    init = function()
      vim.g.barbar_auto_setup = false
    end,
    opts = {},
  },

  {
    --文件列表
    "nvim-tree/nvim-tree.lua",
    version = "*",
    dependencies = {
      "nvim-tree/nvim-web-devicons",
    },
    opts = {}
  },

  {
    --更好的括号效果
    "HiPhish/rainbow-delimiters.nvim",
    submodules = false,
    main = "rainbow-delimiters.setup",
    opts = {}
  },

  {
    --更好的弹出效果
    "folke/noice.nvim",
    event = "VeryLazy",
    dependencies = {
      "MunifTanjim/nui.nvim",
      "rcarriga/nvim-notify",
    },
    opts = {}
  },
}
