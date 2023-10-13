<script lang='ts'>
	import type { Data } from "$lib/data";
  import Card  from "./card.svelte";
  const tempCardData: Data = {
    upvotes: 56,
    downvotes: 4,
    code: `console.log('lol, this is passed in');`,
    comments: [],
    created: '2023-10-12',
    modified: '2023-10-13',
    snippetTitle: 'The best code eva'
  }
</script>

<div class="main-container">
  <div class="header">
    <h1>Purple Code Platform</h1>
  <p>We write so much code, but sometimes we lose track of the good bits.</p>
  <p>Here we can save the bits that others (or just us) might want to use again later.</p>
  </div>

  <button class='new-button'>Add new</button>
  <ul class="code-area">
    <Card data={tempCardData}/>
  </ul>
</div>
<style src='./page.css'></style>