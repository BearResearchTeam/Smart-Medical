# 药品批量删除功能使用说明

## 功能概述

药品批量删除功能允许用户一次性删除多个药品，提高管理效率。该功能包含安全检查机制，防止误删除重要药品。

## 功能特性

### 1. 安全检查
- **库存检查**：默认情况下，有库存的药品无法删除
- **有效期检查**：默认情况下，在有效期内的药品无法删除
- **强制删除**：可选择强制删除模式，忽略安全检查

### 2. 批量操作
- 支持多选药品进行批量删除
- 提供删除结果反馈
- 支持分页显示药品列表

## API 接口

### 批量删除接口

**请求地址：** `POST /api/app/drug/batch-delete`

**请求参数：**
```json
{
  "drugIds": [1, 2, 3, 4],
  "forceDelete": false
}
```

**参数说明：**
- `drugIds`：要删除的药品ID数组（必填）
- `forceDelete`：是否强制删除，忽略安全检查（可选，默认false）

**响应示例：**
```json
{
  "success": true,
  "code": 200,
  "message": "操作成功",
  "result": null
}
```

**错误响应示例：**
```json
{
  "success": false,
  "code": 400,
  "message": "以下药品还有库存，无法删除：阿莫西林, 布洛芬。如需强制删除，请设置ForceDelete为true",
  "result": null
}
```

## 前端使用

### 1. API 调用

```javascript
import DrugAPI from '@/api/drug'

// 批量删除药品
const batchDeleteData = {
  drugIds: [1, 2, 3],
  forceDelete: false
}

const response = await DrugAPI.batchDeleteDrug(batchDeleteData)
if (response.success) {
  console.log('批量删除成功')
} else {
  console.error('批量删除失败:', response.message)
}
```

### 2. Vue 组件使用

```vue
<template>
  <el-table
    :data="drugList"
    @selection-change="handleSelectionChange">
    <el-table-column type="selection" width="55"></el-table-column>
    <!-- 其他列... -->
  </el-table>
  
  <el-button @click="handleBatchDelete(false)">批量删除</el-button>
  <el-button @click="handleBatchDelete(true)">强制批量删除</el-button>
</template>

<script>
export default {
  methods: {
    async handleBatchDelete(forceDelete) {
      const data = {
        drugIds: this.selectedDrugs.map(drug => drug.drugID),
        forceDelete: forceDelete
      }
      
      const response = await DrugAPI.batchDeleteDrug(data)
      if (response.success) {
        this.$message.success('删除成功')
        this.loadDrugList()
      }
    }
  }
}
</script>
```

## 业务规则

### 1. 删除限制
- **有库存药品**：默认不允许删除，需要设置 `forceDelete=true`
- **有效期药品**：默认不允许删除，需要设置 `forceDelete=true`
- **已过期药品**：可以正常删除
- **零库存药品**：可以正常删除

### 2. 强制删除模式
当设置 `forceDelete=true` 时：
- 忽略库存检查
- 忽略有效期检查
- 直接删除选中的药品

### 3. 错误处理
- 如果部分药品无法删除，会返回详细的错误信息
- 包含无法删除的药品名称和原因
- 提供解决建议（如使用强制删除）

## 安全建议

1. **谨慎使用强制删除**：强制删除会忽略所有安全检查
2. **定期备份数据**：重要操作前建议备份数据
3. **权限控制**：建议对批量删除功能进行权限控制
4. **操作日志**：系统会自动记录删除操作日志

## 注意事项

1. 删除操作不可逆，请谨慎操作
2. 建议在删除前确认药品信息
3. 批量删除操作会记录在系统日志中
4. 删除后需要刷新药品列表

## 常见问题

### Q: 为什么有些药品无法删除？
A: 系统会检查药品的库存和有效期，有库存或在有效期内的药品默认无法删除，需要使用强制删除模式。

### Q: 如何查看删除操作日志？
A: 删除操作会自动记录在系统日志中，可以通过日志管理功能查看。

### Q: 批量删除有数量限制吗？
A: 目前没有硬性限制，但建议单次删除数量不超过100个，以保证系统性能。 