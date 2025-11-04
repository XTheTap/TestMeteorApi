<template>
  <table class="meteorites-table">
    <thead>
      <tr>
        <th @click="$emit('sort', 'meteorName')">Название</th>
        <th @click="$emit('sort', 'recclassText')">Класс</th>
        <th @click="$emit('sort', 'year')">Год</th>
        <th @click="$emit('sort', 'count')">Количество метеоритов</th>
        <th @click="$emit('sort', 'totalMass')">Суммарная масса</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="m in props.data" :key="m.year">
        <td>{{ m.meteorName }}</td>
        <td>{{ m.recclassText }}</td>
        <td>{{ m.year }}</td>
        <td>{{ m.count ?? 0 }}</td>
        <td>{{ (m.totalMass ?? 0).toFixed(2) }}</td>
      </tr>
    </tbody>
  </table>
</template>

<script setup>

const props = defineProps({
  data: Array,
});
</script>

<style scoped>
.meteorites-table {
  width: 100%;
  border-collapse: collapse;
}
th {
  cursor: pointer;
  text-align: left;
  border-bottom: 2px solid #ccc;
}
td {
  padding: 0.5rem;
  border-bottom: 1px solid #eee;
}
</style>