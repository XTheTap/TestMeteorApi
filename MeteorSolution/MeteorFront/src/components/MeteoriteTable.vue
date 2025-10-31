<template>
  <table class="meteorites-table">
    <thead>
      <tr>
        <th @click="sort('year')">Год</th>
        <th @click="sort('count')">Количество метеоритов</th>
        <th @click="sort('totalMass')">Суммарная масса</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="m in sortedData" :key="m.year">
        <td>{{ m.year }}</td>
        <td>{{ m.count }}</td>
        <td>{{ m.totalMass.toFixed(2) }}</td>
      </tr>
    </tbody>
  </table>
</template>

<script setup>
import { ref, computed, watch } from "vue";

const props = defineProps({
  data: Array,
});

const sortBy = ref("");
const sortDir = ref(1);

function sort(column) {
  if (sortBy.value === column) {
    sortDir.value *= -1;
  } else {
    sortBy.value = column;
    sortDir.value = 1;
  }
}

const sortedData = computed(() => {
  if (!sortBy.value) return props.data;
  return [...props.data].sort((a, b) => {
    if (a[sortBy.value] > b[sortBy.value]) return sortDir.value;
    if (a[sortBy.value] < b[sortBy.value]) return -sortDir.value;
    return 0;
  });
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