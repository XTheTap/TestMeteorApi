<template>
  <div class="filters">
    <select v-model="filters.yearFrom">
      <option value="">Год с...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.yearTo">
      <option value="">Год по...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.class">
      <option value="">Класс метеорита</option>
      <option v-for="cls in classes" :key="cls" :value="cls">{{ cls }}</option>
    </select>

    <input
      type="text"
      v-model="filters.name"
      placeholder="Часть названия..."
    />

    <button @click="$emit('apply', filters)">Применить</button>
  </div>
</template>

<script setup>
import { ref } from "vue";

const filters = ref({
  yearFrom: "",
  yearTo: "",
  class: "",
  name: "",
});

const years = Array.from({ length: 2025 - 1800 }, (_, i) => 1800 + i);
const classes = ["Iron", "Chondrite", "Achondrite", "Unknown"];
</script>

<style scoped>
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}
</style>