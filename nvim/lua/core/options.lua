local opt = vim.opt

--行号
opt.relativenumber = true
opt.number = true

--缩进
opt.tabstop = 2
opt.shiftwidth = 2
opt.expandtab = true
opt.autoindent = true

--防止包裹
opt.wrap = false

--光标行
opt.cursorline = true

--启用鼠标
opt.mouse:append("a")

--默认新窗口右和下
opt.splitright = true
opt.splitbelow = true

--搜索
opt.ignorecase = true
opt.smartcase = true

--外观
opt.termguicolors = true
opt.signcolumn = "yes"

--配置文件映射
--将cs后缀的文件映射为c_sharp文件，用以应对不识别cs文件的情况
vim.filetype.add({
  extension = {
    cs = "c_sharp"
  }
})
