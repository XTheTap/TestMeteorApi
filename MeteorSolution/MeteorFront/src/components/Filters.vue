<template>
  <div class="filters">
    <select v-model="filters.YearFrom">
      <option value="">Год с...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.YearTo">
      <option value="">Год по...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <input
      type="text"
      v-model="filters.Recclass"
      placeholder="Класс метеорита..."
    />

    <input
      type="text"
      v-model="filters.NameContains"
      placeholder="Часть названия..."
    />

    <select v-model="filters.SortBy">
      <option value="year">Год</option>
      <option value="count">Количество</option>
      <option value="totalMass">Суммарная масса</option>
    </select>

    <label>
      <input type="checkbox" v-model="filters.Desc" /> По убыванию
    </label>

    <button @click="$emit('apply', { ...filters })">Применить</button>
  </div>
</template>

<script setup>
import { reactive } from "vue";

const filters = reactive({
  YearFrom: "",
  YearTo: "",
  Recclass: "",
  NameContains: "",
  SortBy: "year",
  Desc: false,
});

const years = Array.from({ length: 2025 - 1800 }, (_, i) => 1800 + i);
</script>

<style scoped>
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}
</style>